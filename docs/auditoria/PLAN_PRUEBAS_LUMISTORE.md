# Plan y registro de pruebas Lumi Store

Fecha de actualización: 2026-10-23.

## Resultados de esta revisión

| Prueba | Resultado | Evidencia / observaciones |
|---|---|---|
| Inspección del proyecto | APROBADO | Se conservaron `Tienda`, `Administrador`, `Negocio`, `Datos`, `Entidad`; los cinco declaran .NET Framework 4.7.2. |
| `dotnet build Entidad/Entidad.csproj` | APROBADO | Compilación Debug satisfactoria, sin advertencias ni errores. |
| `dotnet build Negocio/Negocio.csproj` | APROBADO CON ADVERTENCIA | Compiló `Negocio`, `Datos` y `Entidad`; salió una advertencia de variable no utilizada en `CN_Recursos`, luego corregida. Repetir tras el cambio final. |
| `dotnet build LumiStore.slnx --no-restore` | BLOQUEADO POR ENTORNO | No están instalados `Microsoft.WebApplication.targets` para `Tienda` y `Administrador`. En esa primera ejecución faltaban además los assets NuGet de los proyectos de biblioteca. |
| Identificación de base LocalDB | APROBADO | Consulta de solo lectura devolvió `DBCARRITO`. |
| Inventario puntual de objetos SQL | FALLIDO EN CONTRATOS REQUERIDOS | Se encontraron las 11 tablas esperadas y la función `fn_obtenerCarritoCliente`; `sys.procedures` devolvió 0 y `sys.table_types` devolvió 0. No existen los procedimientos de carrito/venta que invoca C#. |
| Scripts SQL completos 00/01 | PENDIENTE | Las consultas sobre `sys.columns` excedieron el tiempo de espera. No se aplicaron cambios. |
| `git diff --check` | PENDIENTE DE REPETIR | Ejecutado durante la revisión parcial; volver a correr al cierre. |
| SMTP y PayPal Sandbox | PENDIENTE | No se envió correo ni se creó/capturó orden. Las credenciales SMTP ahora requieren variables de entorno. |

## Casos requeridos antes de publicar

- Instalar Build Tools/Visual Studio con los targets ASP.NET Web Application y el targeting pack 4.7.2; compilar Debug y Release.
- Revisar bloqueos de metadatos SQL, extraer columnas/TVP con `00_diagnostico_lectura_dbcarrito.sql` y definir procedimientos requeridos en una migración revisada para una base aislada.
- Implementar y verificar PayPal Sandbox de servidor a servidor; validar orden, importe, moneda, estado, duplicados, cancelación y captura antes de registrar ventas o descontar inventario.
- Probar catálogo, login, registro, autorización, carrito, stock, administración, reportes y correo en un entorno de prueba.
- Añadir pruebas negativas de cantidades, manipulación de precio, stock insuficiente, solicitudes repetidas, CSRF y archivos subidos.
- Revisar la configuración de publicación y desactivar `debug` en Release de forma controlada.

Los pagos, correos, scripts SQL y pruebas de navegador no se consideran aprobados hasta que se ejecuten con evidencia.
