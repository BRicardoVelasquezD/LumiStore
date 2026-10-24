# Informe de auditoría Lumi Store

Fecha: 2026-10-23. Alcance: inspección estática, reparaciones locales y compilaciones posibles en el entorno disponible. No se ejecutaron escrituras contra SQL Server ni servicios externos.

## Estado

La solución conserva sus cinco proyectos: `Tienda`, `Administrador`, `Negocio`, `Datos` y `Entidad`. Los proyectos siguen apuntando a .NET Framework 4.7.2. La solución completa no pudo compilarse porque el entorno no tiene instalados los targets de ASP.NET Web Application para Visual Studio; los proyectos de biblioteca sí compilaron.

## Cambios y hallazgos

| Estado | Hallazgo | Acción / evidencia |
|---|---|---|
| Corregido, verificado estáticamente | `TiendaController.ProcesarCompra` respondía éxito e inventaba un identificador sin cobrar ni registrar la venta. | Ahora responde HTTP 503 y deja claro que no hubo cargo ni compra. |
| Corregido, verificado estáticamente | El JavaScript del carrito simulaba una redirección y mostraba una compra de demostración como exitosa. | El flujo informa que el pago no está disponible y no envía la solicitud de compra. |
| Corregido, verificado estáticamente | La ruta administrativa inicial apuntaba al controlador `Tienda`, inexistente en ese proyecto. | El valor inicial ahora es `Home/Index`. |
| Corregido | El código SMTP contenía una contraseña de aplicación incrustada. | Se retiró del código. El envío conserva Gmail, remitente predeterminado, puerto 587 y TLS; las credenciales se leen de `LUMISTORE_SMTP_USERNAME` y `LUMISTORE_SMTP_PASSWORD`. Si faltan, el método no envía correo y devuelve `false`. La cuenta debe rotar/revocar la credencial anterior porque ya estuvo en el repositorio. |
| Pendiente de verificación | Autorización por rol, antifalsificación CSRF, validación del lado servidor y manejo uniforme de errores. | Requiere pruebas en el host web y revisar el flujo completo de vistas/controladores. |
| Pendiente de verificación | Contratos SQL, procedimientos, tipo de tabla para detalles de venta y datos de demostración. | Se entregan diagnósticos de solo lectura; no se creó DDL ni carga de datos sin esquema confirmado. |
| No implementado | PayPal Orders API, captura verificada en servidor y registro transaccional de venta/stock. | No se debe presentar el checkout como funcional. El endpoint actualmente falla de forma segura. |

## Verificaciones ejecutadas

| Verificación | Resultado |
|---|---|
| `dotnet build Entidad/Entidad.csproj` | Aprobada, 0 advertencias y 0 errores. |
| `dotnet build Negocio/Negocio.csproj` | Aprobada junto con `Datos` y `Entidad`; al momento de esa ejecución presentó una advertencia preexistente de variable no usada, luego corregida. Repetir build después del cambio final. |
| `dotnet build LumiStore.slnx --no-restore` | Bloqueada: faltan `Microsoft.WebApplication.targets` para `Tienda` y `Administrador`; los assets de NuGet aún no estaban restaurados en esa ejecución. No equivale a una compilación satisfactoria de la solución. |
| `git diff --check` | Sin errores en los cambios existentes al momento de la inspección; repetir al cierre. |
| SQL Server, SMTP y PayPal Sandbox | No ejecutados. `sqlcmd` está disponible, pero no se encontró servidor por descubrimiento y no se estableció conexión. |

## SQL Server

Se conectó por lectura a LocalDB y se confirmó `DBCARRITO`. Un inventario puntual encontró las once tablas usadas por la aplicación y `fn_obtenerCarritoCliente`, pero cero procedimientos almacenados y cero tipos de tabla. Por tanto, las operaciones SQL de carrito y venta que dependen de SP/TVP no están listas en esta base. Las lecturas completas de columnas en los scripts `00`/`01` agotaron el tiempo de espera; no se ejecutó DDL ni hubo escrituras. No se generó carga de demostración porque faltan contratos verificados de columnas, claves y nulabilidad.

Los scripts `00`, `01` y `04` son de solo lectura; `03` es deliberadamente una plantilla sin DDL. Consulte [README_EJECUCION_SQLSERVER.md](../sql/README_EJECUCION_SQLSERVER.md) para la salida observada y el orden de revisión.

Consulte [README_EJECUCION_SQLSERVER.md](../sql/README_EJECUCION_SQLSERVER.md) para el orden de revisión. No ejecutar una migración en producción sin validar primero el esquema real y probarla en una base aislada.

## Bloqueos para una demostración funcional con venta real de Sandbox

1. Instalar/usar Visual Studio Build Tools con el componente **ASP.NET and web development** y el targeting pack .NET Framework 4.7.2, restaurar dependencias y compilar Debug/Release.
2. Resolver el tiempo de espera de metadatos SQL, confirmar contratos reales y diseñar los procedimientos/TVP faltantes antes de preparar migración o datos de prueba.
3. Implementar creación y captura PayPal Sandbox del lado servidor, validar estado/moneda/importe/orden e idempotencia y registrar venta/inventario en una transacción.
4. Ejecutar pruebas de aplicación contra una base aislada y probar SMTP únicamente con un destinatario autorizado.
5. Configurar `LUMISTORE_SMTP_USERNAME` y `LUMISTORE_SMTP_PASSWORD` en el entorno de ejecución y rotar la credencial SMTP previamente expuesta.

No se declara la solución lista para publicación ni completamente funcional.
