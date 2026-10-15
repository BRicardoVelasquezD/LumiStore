# Progreso — 04-entidad-moderna

## Estado de Entidad
- `Entidad/Entidad.csproj` sigue en SDK-style con `<TargetFramework>net472</TargetFramework>`; no se modificó durante esta tarea.
- No hay una herramienta específica disponible para cambiar el TFM. La herramienta `convert_project_to_sdk_style` solo convierte el formato y ya confirmó que Entidad era SDK-style.
- El usuario indicó que hubo tres intentos fallidos previos de editar este archivo con la solución abierta y ordenó no repetir intentos equivalentes. No edité el `.csproj` ni descargué el proyecto.
- La tarea queda bloqueada hasta que el usuario cierre y vuelva a abrir la solución, o proporcione una vía soportada de edición/recarga. Por ello no se puede validar aún la compilación multi-target `net10.0;net472`.

## Validaciones y trabajo relacionado solicitado
- El usuario ya había cambiado `Datos/CD_Venta.cs` para consultar `VENTA.FechaVenta AS FechaRegistro`. Revisé el cambio y compilé `Datos.csproj`: correcto. La compilación completa de `LumiStore.slnx` también terminó correctamente.
- Ejecuté el diagnóstico de `DBCARRITO` únicamente con consultas SELECT sobre catálogos `sys.*`; no se consultaron filas de negocio ni se hicieron escrituras.
- Resultado confirmado: 11 tablas, 8 PK, 0 FK, 0 procedimientos, 1 función tabular inline (`dbo.fn_obtenerCarritoCliente`), 0 triggers, 0 tipos de tabla de usuario, 0 defaults y 0 checks.
- Se entregaron `docs/sql/diagnostico_catalogo_dbcarrito.sql` (script de catálogo, ejecutado correctamente en modo SELECT) y `docs/sql/inventario-y-contratos-dbcarrito.md` (inventario físico separado de contratos esperados). El informe enumera los 18 procedimientos que el código espera y que no existen en el catálogo; no se crearon ni ejecutaron.
- No se realizaron pruebas CRUD que escriban en `DBCARRITO`. La base permanece sin cambios. No se implementó pago real ni se eligió proveedor.
- SMTP no se modificó; el usuario confirma que la configuración existente es correcta.

## Actualización posterior: DDL autorizado para integridad referencial
- El usuario autorizó expresamente aplicar DDL de relaciones en `DBCARRITO`, sin CRUD/DML de filas. Un preflight SELECT agregado comprobó 0 duplicados/nulos en los tres identificadores geográficos y 0 huérfanos para las relaciones incluidas.
- Se ejecutó `docs/sql/relaciones_seguidas_dbcarrito.sql` en una transacción: se añadieron PK a CIUDAD, DEPARTAMENTO y MUNICIPIO y 10 FK `NO ACTION`. No se modificaron filas de negocio.
- La consulta posterior confirmó 11 PK, 10 FK habilitadas/confiables; los diagnósticos siguen mostrando 0 procedimientos y 0 tipos TVP. `VENTA.IdCiudad`→`CIUDAD.IdCiudad` se dejó fuera por discrepancia `varchar(10)`/`varchar(6)`.
- `LumiStore.slnx` compila correctamente tras el DDL. No se ejecutó ningún CRUD escribiente.
- Esto no desbloquea esta tarea: `Entidad.csproj` sigue en `net472` y la solución todavía no se ha cerrado/reabierto. La tarea 04 continúa fallida/bloqueada.

## Resultado
- Tarea 04: bloqueada/fallida por el requisito de cambiar el TFM de Entidad sin repetir una vía de edición prohibida. No avanzar a las tareas dependientes hasta recibir la acción del usuario.
