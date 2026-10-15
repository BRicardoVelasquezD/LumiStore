# Resultado de auditoría exhaustiva (solo lectura)

## Alcance y método
Se amplió la auditoría de Tienda/Administrador solicitada para abarcar compatibilidad con .NET 10, librerías/datos, tests, configuración, riesgos de seguridad y evidencias locales de CI/CD/Azure/despliegue. Se usaron el assessment MCP ya generado, inventario recursivo de archivos, búsquedas estáticas y lectura de código/configuración. No se modificó código de la aplicación, datos ni infraestructura; no se autenticó con Azure ni se desplegó.

## Hechos confirmados
- Cinco proyectos target .NET Framework 4.7.2; los hosts web son MVC 5/System.Web clásicos.
- El assessment estático reporta 1.764 issues (645 obligatorios, 1.115 potenciales y 4 opcionales), con 614 relacionados con ASP.NET Framework/System.Web; 578 incompatibilidades binarias, 1.080 de fuente y 45 observaciones de binding redirects. Datos concentra 998 incidencias (996 ADO.NET heurísticas); Tienda 431, Administrador 331; Entidad y Negocio 2 cada uno. Estos resultados son indicios del assessment, no fallos runtime confirmados.
- Paquetes marcados incluyen AngleSharp 0.17.1 como vulnerable en ambos proyectos (assessment sugiere 1.8.4, a validar con AngleSharp.Css 0.17.0 y HtmlSanitizer 9.0.892), Antlr 3.5.0.2 y dependencias ASP.NET MVC/Razor/WebPages incompatibles para el destino indicado. No se actualizaron paquetes.
- El checkout en `Tienda/Views/Tienda/Carrito.cshtml` simula el pago/confirmación (incluida referencia de PayPal), no se encontró invocación a `ProcesarCompra`; este action requiere cliente en sesión pero devuelve éxito/ID sintéticos y no llama a persistencia. `Datos/CD_Venta.cs` sí contiene ruta hacia `usp_RegistrarVenta`, sin conexión demostrada al action. No se ejecutó escritura ni transacción financiera.
- `Negocio/CN_Recursos.cs` contiene credenciales SMTP literales. Por seguridad no se reproduce aquí. La contraseña debe tratarse como comprometida: revocación/rotación urgente por el propietario, revisión de distribución/historial y migración a secreto de entorno/gestor antes de despliegue. No fue usada.
- El mismo archivo calcula SHA-256 determinista directo para contraseñas, sin salt ni KDF; revisar plan de migración segura con rehash.
- El controlador de mantenimiento administrativo está protegido por `[Authorize]`, sin roles visibles en esa clase; la búsqueda estática encontró tokens antiforgery en dos vistas de Tienda y ninguno en vistas de Administrador, y no encontró atributos `[ValidateAntiForgeryToken]` en controllers. Verificar todos los AJAX POST y filtros globales antes de concluir exposición. Subida de producto escribe a `Imagenes/Productos` sin validación de tipo/tamaño visible en el método inspeccionado.
- Ambos Web.config apuntan a LocalDB `DBCARRITO`; el target es 4.7.2 y la compilación base tiene debug=true. Los transforms Release quitan debug; ejemplos de cadena productiva y customErrors permanecen comentados. No queda demostrado ningún endpoint/base de producción.
- En el árbol local inspeccionado no aparecen definiciones versionadas de GitHub Actions/Azure Pipelines, contenedores, Azure YAML, IaC Terraform/Bicep, manifests/scripts/perfiles de publicación ni global.json/NuGet.Config relacionados con deployment. No se encontró integración Azure de aplicación en los archivos revisados; Azure CLI no está disponible. `.mcp.json` solo declara servidores MCP Azure/GitHub; no acredita conexión ni despliegue de LumiStore.
- Por tanto, el nombre/ubicación del despliegue real es **no verificable desde el repositorio/local**. No se puede inferir que la organización carezca de pipelines externas ni que la aplicación no esté alojada en Azure. Hace falta evidencia autorizada del propietario (pipeline/servicio/URL/documentación o acceso de lectura).
- Build previo de la solución fue correcto en el target actual; assessment no equivale a build net10.0. No hay proyectos de tests detectados. Existió respuesta HTTP 200 de listado Tienda en una comprobación previa; no se probó admin autenticado ni CRUD. Hay media queries/meta viewport, pero no prueba visual responsive. No se ejecutó checkout.

## Prioridad recomendada
1. **Crítica / inmediata**: tratar la credencial SMTP incrustada como comprometida; propietario revoca y rota, comprueba exposición/historial, la retira de código y la mueve a almacenamiento/configuración de secretos, sin incluir el valor en tickets/commits nuevos.
2. **Bloqueante de producción**: identificar destino y responsable de despliegue; definir pipeline versionada/aprobaciones/rollback o aportar evidencia de pipeline administrada externamente. Separar connection strings/secretos por entorno. El LocalDB actual y transform Release no configuran un entorno productivo.
3. **Alta**: impedir toda falsa confirmación de pedido; conectar y verificar el flujo servidor-datos con pruebas aisladas y sin pagos reales hasta decisión sobre proveedor. No usar importes calculados en cliente como confiables.
4. **Alta**: revisar autenticación/autorización por rol, antiforgery en todos los POST AJAX, hashing/password reset y límites/validación de ficheros. Un análisis estático ha marcado gaps, pero el contexto completo (filtros/scripts) aún debe comprobarse.
5. **Alta**: resolver vulnerabilidad AngleSharp luego de confirmar la matriz de compatibilidad completa; evaluar restantes paquetes vulnerables/incompatibles según assessment.
6. **Estructural**: abordar System.Web/MVC5, ADO.NET y binding redirects mediante la migración planificada y validación por proyecto; no sustituirlos en masa.
7. **QA**: añadir pruebas automatizadas; comprobar checkout falso/bloqueo, autorización de roles, CSRF, entrada/salida y cargas; navegador con viewports 320/375/768/1024/1440, lector/teclado y regresión del catálogo.

## Verificaciones realizadas y límites
- **Comprobado**: lectura de assessment; inventario de archivos locales CI/CD/Azure/publicación; búsqueda de referencias Azure; lectura de controllers, helper de contraseña/email, carrito y Release transforms; conteo/búsqueda estática de tokens/rutas/viewport; build y test-project discovery previos; parseo XML previo correcto.
- **No comprobado**: pipeline interna de una organización fuera de checkout, suscripción/recursos Azure, dominio/URL productivo, settings del servicio hospedado, prueba visual real en navegador, compra end-to-end, escritura CRUD, pago real, cambios SQL, sesión de administrador autenticada y efectividad de filtros globales antiforgery.
- **Acción del usuario/propietario**: revocar/rotar el secreto SMTP inmediatamente; facilitar documentación/nombre del target/pipeline si desea verificar Azure externo; decidir proveedor de pagos por separado y autorizar expresamente cualquier despliegue o cambio de framework/arquitectura.

## Validación de esta tarea
- No se cambiaron archivos funcionales, dependencias, configuración de despliegue, datos ni recursos externos.
- La investigación es documental/estática; build de base de la solución previamente exitoso; no hay tests de repositorio detectados.
- Limitaciones explícitas para QA runtime, Azure externo y despliegue comunicadas arriba.
