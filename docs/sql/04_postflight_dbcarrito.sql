/* Organiza sección */
SET NOCOUNT ON;
SELECT DB_NAME() AS BaseActual;
SELECT s.name AS Esquema,o.name AS Objeto,o.type_desc AS Tipo
FROM sys.objects AS o JOIN sys.schemas AS s ON s.schema_id=o.schema_id
WHERE o.name IN ('PRODUCTO','CATEGORIA','MARCA','CARRITO','VENTA','DETALLE_VENTA','CLIENTE','USUARIO',
 'DEPARTAMENTO','MUNICIPIO','CIUDAD','fn_obtenerCarritoCliente','sp_ExisteCarrito',
 'sp_OperacionCarrito','sp_EliminarCarrito','usp_RegistrarVenta')
ORDER BY o.name,s.name;
SELECT p.name AS Procedimiento, prm.parameter_id, prm.name AS Parametro,
       TYPE_NAME(prm.user_type_id) AS Tipo, prm.max_length, prm.is_output
FROM sys.procedures AS p LEFT JOIN sys.parameters AS prm ON prm.object_id=p.object_id
WHERE p.name IN ('sp_ExisteCarrito','sp_OperacionCarrito','sp_EliminarCarrito','usp_RegistrarVenta')
ORDER BY p.name,prm.parameter_id;
SELECT s.name AS Esquema,tt.name AS TipoTabla,c.column_id,c.name AS Columna,
       TYPE_NAME(c.user_type_id) AS Tipo,c.max_length,c.precision,c.scale,c.is_nullable
FROM sys.table_types AS tt JOIN sys.schemas AS s ON s.schema_id=tt.schema_id
JOIN sys.columns AS c ON c.object_id=tt.type_table_object_id
ORDER BY s.name,tt.name,c.column_id;
