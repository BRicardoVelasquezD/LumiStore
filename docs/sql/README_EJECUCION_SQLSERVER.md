# Diagnóstico SQL Server de Lumi Store

Conectarse en SSMS a la instancia autorizada y seleccionar explícitamente `DBCARRITO`. Los scripts `00_diagnostico_lectura_dbcarrito.sql`, `01_preflight_integridad_dbcarrito.sql` y `04_postflight_dbcarrito.sql` son de solo lectura. `03_migracion_objetos_faltantes_dbcarrito.sql` es una plantilla que no crea ni altera objetos.

## Hallazgos comprobados en LocalDB

El 2026-10-23 se conectó con `sqlcmd` a `(localdb)\MSSQLLocalDB`, base `DBCARRITO`. Consultas de metadatos puntuales encontraron las tablas `PRODUCTO`, `CATEGORIA`, `MARCA`, `CARRITO`, `VENTA`, `DETALLE_VENTA`, `CLIENTE`, `USUARIO`, `DEPARTAMENTO`, `MUNICIPIO` y `CIUDAD`, además de `fn_obtenerCarritoCliente`. La base reportó cero procedimientos almacenados y cero tipos de tabla definidos por el usuario. Esto deja sin contrato desplegado las operaciones de carrito y venta que invoca la aplicación.

Las consultas completas de los scripts `00`/`01` no terminaron: la lectura de `sys.columns` excedió el tiempo de espera. No se ejecutaron escrituras. Antes de crear los objetos faltantes, revisar los metadatos en SSMS y definir el contrato del TVP de detalles con el responsable de la base.

## Orden de trabajo

1. Ejecutar `00_diagnostico_lectura_dbcarrito.sql`; guardar la salida de metadatos, sin extraer datos personales.
2. Ejecutar `01_preflight_integridad_dbcarrito.sql`; cotejar los objetos y columnas con `02_matriz_contratos_sql.md`.
3. Preparar y revisar DDL de procedimientos y tipo TVP en una base aislada. El archivo `03_migracion_objetos_faltantes_dbcarrito.sql` aún no contiene esa migración.
4. No ejecutar cambios en producción sin respaldo, revisión y autorización del responsable de datos.
5. Después de una migración aprobada y ejecutada manualmente, correr `04_postflight_dbcarrito.sql` y conservar la evidencia.

No se agregan datos de demostración hasta comprobar nulabilidad, claves, restricciones, columnas obligatorias y contratos reales. No ejecutar scripts de escritura inferidos.
