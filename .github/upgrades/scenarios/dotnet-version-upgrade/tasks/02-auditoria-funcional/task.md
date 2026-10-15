# 02-auditoria-funcional: Auditoría exhaustiva de compatibilidad, seguridad y despliegue

## Objetivo
Realizar una auditoría de solo lectura de la solución .NET Framework 4.7.2: incidencias de compatibilidad con `net10.0`, aplicación Tienda/Administrador, flujo de compra, seguridad/configuración, pruebas, CI/CD, Azure e identificación del destino/proceso de despliegue. Separar hechos locales demostrables, riesgos, y elementos que no pueden confirmarse sin acceso externo. No cambiar framework/arquitectura, código de aplicación, datos, credenciales, recursos Azure ni desplegar.

## Inventario de alcance
- **Proyectos**: Entidad, Datos, Negocio, Tienda y Administrador; todos targeting .NET Framework 4.7.2. Tienda y Administrador son aplicaciones ASP.NET MVC 5/System.Web clásicas; las bibliotecas y referencias también son proyectos clásicos.
- **Concerns**: compatibility assessment, paquetes y binding redirects; compra/pago; autenticación/CSRF/subidas/secretos; conexión y transformaciones Release; cobertura de pruebas; configuración y evidencia local de pipeline/IaC/publicación; límites de inspección del Azure remoto.
- **Responsabilidades**: Backend: evaluar dependencias, endpoints, autenticación, datos, pagos y compatibilidad. Frontend: catálogo/carrito/vistas, idioma y evidencia estática de adaptación. QA: build, inventario de pruebas, HTTP previamente probado, límites de pruebas visuales/autenticadas y casos pendientes. No se realizaron cambios de aplicación.

## Hallazgos de investigación confirmados

### Compatibilidad (assessment estático para net10.0; no es autorización de migración)
- Assessment MCP: cinco proyectos, 1.764 incidencias estimadas (645 obligatorias, 1.115 potenciales, 4 opcionales), 31 archivos afectados, y 614 asociadas a ASP.NET Framework/System.Web.
- Distribución del assessment: Datos 998 hallazgos (incluye 996 heurísticos ADO.NET y 2 de proyecto); Tienda 431 (335 obligatorios); Administrador 331 (304 obligatorios); Entidad y Negocio 2 cada uno. 578 incompatibilidades binarias y 1.080 de fuente reportadas, además de 45 observaciones de binding redirects. Son hallazgos estáticos/heurísticos, no conteo de fallos runtime; revisar y validar cada API en su contexto.
- MVC 5, System.Web, sesiones, autenticación Forms, rutas/filtros y bundles tienen diferencias arquitectónicas con ASP.NET Core; el assessment reporta numerosos símbolos MVC/System.Web incompatibles. No se inició conversión.
- El assessment marca cinco paquetes incompatibles y tres recomendaciones de actualización; identifica `AngleSharp` 0.17.1 con vulnerabilidad en ambos hosts y propone 1.8.4. La solución usa también `AngleSharp.Css` 0.17.0 y `HtmlSanitizer` 9.0.892; la actualización necesita comprobar compatibilidad conjunta. `Antlr` 3.5.0.2 se señala para reemplazo. Los paquetes MVC/Razor/WebPages en español y Optimization plantean incompatibilidades para el destino moderno; no se actualizaron.
- En Tienda y Administrador se informan binding redirects faltantes, conflictivos o que fuerzan versiones inferiores. Inspeccionar/validar redirects y grafo al migrar; no eliminarlos sin probar dependencias.

### Aplicación, transacción y datos
- `Tienda/Views/Tienda/Carrito.cshtml`: la interfaz anuncia PayPal, calcula importes en cliente y presenta un flujo temporizado/simulado; no se encontró llamada del checkout a `ProcesarCompra`. El DOM incorpora propiedades de producto interpoladas en HTML con jQuery `.html()`: revisar origen/encoding y riesgo XSS antes de corregir.
- `Tienda/Controllers/TiendaController.cs`, `ProcesarCompra(Venta)`: requiere `Session["Cliente"]`, pero la implementación observada no llama a la capa de negocio/persistencia; responde éxito y genera identificador sintético. Es falsa confirmación de compra. No se ejecutó la operación.
- `Datos/CD_Venta.cs`, `Registrar`: existe una ruta separada que invoca `usp_RegistrarVenta`, sin vínculo demostrado desde el endpoint de checkout inspeccionado. No se ejecutó SQL de escritura.
- Los datos de envío y los precios del navegador no se consideran fuente confiable; la futura implementación debe recalcular/prevalidar en servidor y coordinar con proveedor solo tras decisión explícita. No seleccionar proveedor ni activar pagos reales.

### Seguridad y configuración
- **Secreto expuesto, prioridad crítica**: `Negocio/CN_Recursos.cs` contiene una cuenta y una credencial SMTP literales en el código. Valores deliberadamente omitidos de este informe. Tratar la credencial como comprometida: el propietario debe revocarla/rotarla, revisar exposición/historial y reemplazarla por configuración secreta segura antes de cualquier despliegue. No fue usada ni copiada a otros archivos.
- `CN_Recursos.ConvertirSha256` aplica SHA-256 directo y determinista a contraseña UTF-8, sin salt ni función lenta de derivación. Riesgo de almacenamiento de contraseñas; planificar migración con verificación/rehash compatible y estrategia de recuperación.
- Login/admin usa Forms Authentication. `Administrador/Controllers/MantenimientoController.cs` tiene `[Authorize]`, pero no se encontró autorización por roles en esa clase; confirmar matriz de permisos y proteger operaciones por rol antes de exponer administración.
- Se encontraron tokens `@Html.AntiForgeryToken()` en dos vistas de Tienda; no se encontraron en vistas de Administrador. La búsqueda estática no encontró `[ValidateAntiForgeryToken]` en controladores MVC. Revisar exhaustivamente emparejamiento/validación de token en todos los POST (incluidos AJAX); no afirmar cobertura ausente en runtime sin comprobar clientes/filtros globales.
- Los endpoints POST de mantenimiento (guardar/eliminar) mutan datos y se encuentran bajo `[Authorize]`; los JSON usan `JsonRequestBehavior.AllowGet`. El guardado de producto escribe subidas a `Imagenes/Productos` usando extensión de nombre y nombre derivado del ID, sin validación de contenido/tamaño visible en el método inspeccionado; requiere control de tipo, tamaño, nombre seguro y autorización de rol. La superficie completa y filtros globales requieren revisión antes de corrección.
- Ambos `Tienda/Web.config` y `Administrador/Web.config` apuntan a `(localdb)\\MSSQLLocalDB`, base `DBCARRITO`, Integrated Security y tienen `compilation debug="true"` para configuración base. Esto confirma configuración local, no un servidor de producción.
- Ambos `Web.Release.config` eliminan el atributo `debug`; el bloque de ejemplo para connection string y `customErrors` está comentado. No se observó sustitución activa de conexión de producción ni transformación activa de errores. No se puede desplegar confiablemente a un host sin configurar conexión/identidad, errores y secretos por entorno.
- Seguridad de cookies/HTTPS, TLS, encabezados, CSP, política de sesión, copias de seguridad y configuración de producción no se pudieron certificar solo con lo revisado; incluirlos en la puerta predespliegue.

### CI/CD, Azure y destino de despliegue
- Inventario de solo lectura en el árbol del workspace: no se localizaron `.github/workflows`, Azure Pipelines YAML, Dockerfile/Compose, `azure.yaml`, `.azure`, infraestructura Terraform/Bicep, manifiestos de publicación, scripts de despliegue, perfiles `.pubxml`/publishsettings, `global.json` o `NuGet.Config` relacionados con CI/CD/deploy.
- No se localizó integración/código de Application Insights, Azure SQL/Storage ni configuración de destino Azure en los archivos inspeccionados. Azure CLI no está disponible en el entorno observado. No se autenticó ni se consultaron suscripciones/recursos.
- `.mcp.json` declara procesos MCP llamados Azure y GitHub; esta declaración configura herramientas MCP, no es evidencia de que la app esté conectada a Azure, de que exista un pipeline, ni de cuál sería el host de producción.
- **Conclusión acotada**: el repositorio inspeccionado no contiene evidencia versionada suficiente para identificar el destino o proceso real de despliegue. No afirmar que no exista un pipeline externo de organización ni que la app no esté desplegada. Para verificar ese hecho hacen falta URL/nombre del entorno, acceso de solo lectura a la configuración de pipeline/servicio, documentación del responsable o exportación autorizada. No se solicitó token, no hubo acceso remoto y no se desplegó.

### Pruebas y UX
- Build de la solución comprobado previamente como correcto con MSBuild/Visual Studio para la configuración existente. No significa compatibilidad con .NET 10 ni validación de Release.
- Descubrimiento de proyectos de prueba: cero proyectos detectados. No hay suite automatizada del repositorio reconocida.
- Prueba HTTP previa: Tienda respondió HTTP 200 a solicitud POST de listado de productos; una página protegida de Administrador solo presentó acceso anónimo/login y no se probó sesión autenticada ni CRUD.
- Hay meta viewport y media queries en varias vistas de Tienda y viewport en el layout/admin; esto es evidencia de intención CSS, no verificación visual/accessibility en móvil. No se ejecutó navegador ni QA responsive. No se realizó flujo de checkout.
- XML de solución, proyectos y configuraciones revisadas validó como XML. No se hizo inspección exhaustiva de cada cadena visible para garantizar español.

## Validación y siguientes pasos
- Mantener esta tarea como auditoría, no como remediación: sin archivos funcionales alterados, cambios de paquetes, migraciones, datos de prueba destructivos, autenticación Azure o despliegues.
- Verificación local disponible: lectura estática, assessment MCP y build existente. Cobertura pendiente de forma explícita: tests automatizados inexistentes; navegador/viewport, checkout, operaciones admin autenticadas y pipeline/Azure remoto no verificados.
- Antes de producción, bloquear hasta revocar el secreto expuesto, designar destino, separar configuración/secretos por entorno, diseñar CI con restore/build/tests/security/deploy approvals/rollback, habilitar logging/health/backup y validar controles de autorización/CSRF/uploads/pago. Estas son recomendaciones; no se aplicaron.
