# Plan de actualización .NET y mejora integral de LumiStore

## Overview

**Objetivo**: preparar la migración de los cinco proyectos .NET Framework 4.7.2 hacia .NET 10, y corregir y verificar la experiencia integral de Tienda y Administrador: vistas, lógica de servidor, flujo de compra/pago, autorización, idioma español, adaptación a móvil y preparación para despliegue.
**Alcance**: solución de cinco proyectos clásicos (Entidad, Datos, Negocio, Tienda y Administrador), con dos aplicaciones ASP.NET MVC 5/System.Web y una base LocalDB `DBCARRITO`. La evaluación existente informa 1.764 incidencias de compatibilidad, cinco paquetes incompatibles y ausencia de proyectos de pruebas; la actualización de framework requiere migración incremental y validaciones explícitas.

### Estrategia seleccionada
**Bottom-Up (de dependencias a aplicaciones)** — migrar primero las bibliotecas y después las aplicaciones web, manteniendo la solución anterior disponible durante el trabajo en paralelo.
**Motivo**: los cinco proyectos están en .NET Framework 4.7.2 y forman una cadena de dependencias de varios niveles; las aplicaciones web MVC 5/System.Web tienen riesgos altos y no se convierten en aplicaciones ASP.NET Core mediante un simple cambio de destino.

```text
Nivel 4: Tienda                 Administrador
			 \                  /
Nivel 3:       Negocio
				  |
Nivel 2:         Datos
				  |
Nivel 1:        Entidad
```

### Criterios por nivel
- **Nivel 1 — Entidad**: biblioteca sin referencias internas; conservar consumidores existentes y habilitar el destino moderno cuando su compatibilidad esté probada.
- **Nivel 2 — Datos**: depende de Entidad; validar consultas, parámetros y conexión a `DBCARRITO` sin cambios de esquema no autorizados.
- **Nivel 3 — Negocio**: depende de Datos y Entidad; verificar reglas y contratos utilizados por ambas aplicaciones.
- **Nivel 4 — Tienda y Administrador**: aplicaciones independientes que consumen las bibliotecas; construir, probar y comparar cada ruta funcional antes de considerar un cambio de tráfico o despliegue.

### Restricciones de pruebas y datos
- El usuario autorizó ejecutar el plan y las verificaciones funcionales/CRUD. Comenzar por build, pruebas automatizadas y consultas de lectura; no ejecutar pruebas que creen, alteren o borren datos en `DBCARRITO` actual. Los casos CRUD con escritura requieren una base de pruebas aislada, copias verificadas y limpieza controlada; si no está disponible, documentar el bloqueo en vez de probar contra datos reales.
- No registrar, mostrar ni reutilizar la credencial SMTP incrustada previamente detectada. Tratarla como expuesta; la rotación corresponde al propietario. No subir secretos a configuración versionada.
- No ejecutar pagos reales ni seleccionar proveedor de pagos; no autenticarse contra Azure, crear recursos ni desplegar sin autorización explícita adicional.

### Estado de ejecución
El usuario autorizó implementar el plan existente con pruebas funcionales y CRUD. La autorización no revoca la protección de datos: las escrituras CRUD requieren una base de pruebas aislada; la base `DBCARRITO` local se ha comprobado únicamente mediante `SELECT` y no se usará para pruebas que escriban.

## Upgrade Options

| Opción | Selección confirmada | Motivo |
|--------|----------------------|--------|
| Estrategia | Bottom-Up | Los cinco proyectos .NET Framework tienen una cadena de dependencias y requieren validación incremental. |
| Aplicaciones web | Side-by-side | Mantiene disponibles las aplicaciones actuales mientras se construyen y verifican las sustitutas ASP.NET Core. |
| Bibliotecas | Multi-targeting | Permite atender a los consumidores .NET Framework existentes y a los nuevos hosts durante la transición. |
| Gestión de paquetes | Por proyecto; aplazar CPM | Reduce fricción mientras coexisten proyectos clásicos, destinos y dependencias distintas. |
| Paquetes incompatibles | Resolver durante el trabajo | La evaluación informa cinco incompatibilidades; cada sustitución debe comprobarse por uso real y compatibilidad. |
| API incompatibles | Corregir durante el trabajo | La evaluación señala cambios binarios y de código que deben resolverse y validarse en cada nivel. |
| System.Web Adapters | Usar adaptadores donde proceda | Facilitan la transición de capacidades System.Web sin reemplazar la arquitectura de forma opaca. |
| Autenticación entre aplicaciones | Autenticación remota | Es la selección registrada para el límite entre las aplicaciones Framework y las nuevas. La mitad Framework y el intercambio seguro deben validarse antes de migrar usuarios. |
| Redirecciones de ensamblado | Documentar y revisar antes de retirar | La evaluación detectó conflictos y redirecciones faltantes; no se eliminarán a ciegas. |
| Referencias anulables | Mantener desactivadas inicialmente | Se evita mezclar una migración masiva de nulabilidad con la compatibilidad y la corrección funcional. |

## Tasks

### 01-requisitos-y-linea-base: Verificar herramientas y línea base

Confirmar SDK, MSBuild, referencias de .NET Framework, configuración de solución y capacidad de restaurar y compilar sin alterar destinos ni código. Registrar la compilación de referencia, los flujos que pueden verificarse en IIS Express y la ausencia o disponibilidad de pruebas automatizadas. QA documentará resultados reproducibles y las limitaciones del entorno local.

**Hecho cuando**: quedan documentados SDK/herramientas, configuración y compilación de referencia; se conocen las pruebas disponibles; no se han cambiado TFM, arquitectura, datos ni recursos remotos.

---

### 02-auditoria-funcional: Auditar vistas, compra y diseño adaptable

Realizar una auditoría inicial de solo lectura en Tienda y Administrador para identificar defectos reproducibles y alcance real: vistas y scripts que fallan, navegación, formularios, cadenas visibles que no estén en español, adaptación móvil, validaciones, errores HTTP y controles de acceso. Trazar el flujo de carrito y pago existente en el código y la configuración sin ejecutar operaciones financieras ni persistir datos. Frontend registrará problemas de interfaz y evidencia en tamaños de pantalla; Backend mapeará rutas, reglas, seguridad y dependencias de pago; QA preparará casos de aceptación y una clasificación de hallazgos con su reproducción. No corregir indiscriminadamente antes de priorizar y no adivinar el proveedor de pago.

**Hecho cuando**: quedan registradas las rutas/archivos y los defectos comprobados con pasos de reproducción, alcance actual del flujo de pago, riesgos y pruebas manuales/automáticas factibles; no se modificó código, datos ni recursos externos.

---

### 03-bibliotecas-sdk: Convertir las bibliotecas clásicas a formato SDK

Convertir Entidad, Datos y Negocio a formato SDK manteniendo inicialmente .NET Framework 4.7.2 y los contratos actuales. Frontend no aplica en esta tarea; Backend revisará referencias, recursos y dependencias NuGet, y QA comprobará compilación de las tres bibliotecas y consumidores actuales. No incluir las aplicaciones web MVC 5 en esta conversión.

**Hecho cuando**: las tres bibliotecas compilables conservan sus contratos y destino Framework; Tienda y Administrador siguen compilando y ninguna referencia necesaria se pierde.

---

### 04-entidad-moderna: Preparar Entidad para el destino moderno

Habilitar y validar el destino moderno para Entidad, que no tiene referencias internas y es la dependencia de Datos, Negocio y ambas aplicaciones. Mantener el contrato compatible con los consumidores existentes. Backend verificará los modelos y sus serializaciones; QA compilará los destinos aprobados y los consumidores Framework antes de avanzar.

**Hecho cuando**: Entidad compila en sus destinos aprobados sin errores ni advertencias y no rompe las referencias de consumidores existentes.

---

### 05-datos-modernos: Habilitar y validar Datos

Habilitar el destino moderno de Datos después de validar Entidad. Investigar las incompatibilidades de ADO.NET señaladas por la evaluación, revisar consultas y parámetros por uso real y conservar el comportamiento de lectura/escritura autorizado. No modificar el esquema ni la cadena LocalDB `DBCARRITO` sin evidencia y aprobación; `#skill:managing-shared-database-schema`. Backend verificará operaciones de datos y QA probará las rutas de acceso con datos controlados antes de continuar.

**Hecho cuando**: Datos compila para los destinos aprobados, conserva consumidores Framework, las pruebas de acceso a base aprobadas pasan y no se alteró el esquema compartido.

---

### 06-negocio-moderno: Habilitar y validar Negocio

Habilitar el destino moderno de Negocio después de validar sus bibliotecas dependientes, Datos y Entidad. Backend comprobará reglas y contratos consumidos por las dos aplicaciones; QA ejecutará pruebas automatizadas y de regresión disponibles. Resolver incidencias de API con base en el informe y en el comportamiento esperado, sin cambios funcionales implícitos.

**Hecho cuando**: Negocio compila para los destinos aprobados, ambas aplicaciones Framework siguen consumiendo los contratos y las pruebas de reglas pasan.

---

### 07-andamiaje-tienda: Crear el host nuevo para Tienda

Crear la aplicación ASP.NET Core paralela de Tienda con proxy incremental, configuración aislada y respuesta inicial comprobable, sin retirar ni interrumpir el host MVC 5. Frontend acordará idioma español, estructura y recursos compartibles; Backend establecerá límites de configuración y acceso a datos con el esquema compartido protegido; QA comprobará el arranque del host nuevo y del antiguo. La conexión a base y el esquema son compartidos: `#skill:managing-shared-database-schema`.

**Hecho cuando**: ambos hosts siguen disponibles, el nuevo responde a la ruta inicial, la configuración no contiene secretos y no se ha modificado el esquema.

---

### 08-tienda-integral: Migrar y corregir la experiencia de compra

Migrar y corregir incrementalmente las vistas, scripts, estilos, rutas y capacidades funcionales de Tienda al host nuevo. Frontend verificará español, diseño responsive, accesibilidad básica, navegación, formularios y estados de carga/error; Backend revisará validaciones, reglas de negocio, autorización y persistencia; QA realizará pruebas de regresión y flujos de extremo a extremo. Inspeccionar los defectos y el código de pago existente antes de decidir su alcance. No elegir proveedor de pago, usar credenciales reales ni activar cobros live sin autorización explícita; si falta información del proveedor, dejar esa integración como bloqueo documentado. El esquema de base es compartido: `#skill:managing-shared-database-schema`.

**Hecho cuando**: las rutas migradas funcionan y las pruebas pasan; la interfaz se presenta en español y es usable en móvil y escritorio; validaciones y errores son coherentes; los cambios del flujo de pago se limitan al nivel respaldado por evidencia y autorización; no hay transacciones reales no autorizadas.

---

### 09-andamiaje-administrador: Crear el host nuevo de Administración

Crear la aplicación ASP.NET Core paralela para Administrador, manteniendo el host MVC 5 existente. Backend revisará autenticación y autorización y preparará el límite de autenticación remota sin inventar usuarios, tokens ni secretos; Frontend establecerá componentes administrativos en español y responsive; QA comprobará el arranque y rechazo de acceso no autorizado sin operaciones CRUD. El esquema de base es compartido: `#skill:managing-shared-database-schema`.

**Hecho cuando**: el host nuevo arranca, la ruta pública responde, las rutas protegidas requieren autorización, no se exponen secretos y el esquema sigue intacto.

---

### 10-administrador-integral: Migrar y corregir Administración

Migrar las vistas, formularios y operaciones administrativas de forma incremental. Frontend revisará idioma español, diseño adaptable, accesibilidad y estados; Backend verificará reglas, controles de acceso por rol, validación de servidor y persistencia; QA probará casos permitidos y denegados con identidades autorizadas y datos controlados. No retirar `[Authorize]`, inventar permisos ni ejecutar cambios de datos destructivos. El esquema de base es compartido: `#skill:managing-shared-database-schema`.

**Hecho cuando**: las capacidades aprobadas respetan roles/permisos, el acceso no autorizado queda bloqueado, las vistas son responsive y españolas, y QA verifica que los cambios persistidos corresponden solo a acciones previstas.

---

### 11-validacion-y-preparacion: Validar la solución y documentar el despliegue

Ejecutar compilación de solución y pruebas automatizadas/manuales disponibles; comprobar los flujos principales de ambas aplicaciones en IIS Express, integración con base de datos y regresión frontend en móvil y escritorio. Resolver todas las advertencias en proyectos modificados. Documentar en español configuración por entorno, publicación, dependencias externas, controles previos, observabilidad y reversión. Revisar antes de retirar los redirects de ensamblado identificados por la evaluación. Recomendar Central Package Management solo después de la estabilización con proyectos SDK en un TFM único. Esta tarea prepara el despliegue; no publica ni conecta Azure.

**Hecho cuando**: build y pruebas registran resultados reproducibles, no quedan errores/advertencias en proyectos tocados, los bloqueos de pago/identidad/infraestructura se documentan, existe guía operativa en español y no se ejecutó despliegue.
