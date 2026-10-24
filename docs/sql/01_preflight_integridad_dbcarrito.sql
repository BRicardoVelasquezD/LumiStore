/* Organiza sección */
SET NOCOUNT ON;
SELECT DB_NAME() AS BaseActual;

DECLARE @Objetos TABLE (Nombre sysname NOT NULL, TipoEsperado varchar(20) NOT NULL);
INSERT INTO @Objetos (Nombre, TipoEsperado) VALUES
('PRODUCTO','TABLE'),('CATEGORIA','TABLE'),('MARCA','TABLE'),('CARRITO','TABLE'),
('VENTA','TABLE'),('DETALLE_VENTA','TABLE'),('CLIENTE','TABLE'),('USUARIO','TABLE'),
('DEPARTAMENTO','TABLE'),('MUNICIPIO','TABLE'),('CIUDAD','TABLE'),
('fn_obtenerCarritoCliente','FUNCTION'),('sp_ExisteCarrito','PROCEDURE'),
('sp_OperacionCarrito','PROCEDURE'),('sp_EliminarCarrito','PROCEDURE'),
('usp_RegistrarVenta','PROCEDURE');

SELECT x.Nombre, x.TipoEsperado,
       CASE WHEN x.TipoEsperado='TABLE' AND t.object_id IS NOT NULL THEN 'PRESENTE'
            WHEN x.TipoEsperado='PROCEDURE' AND p.object_id IS NOT NULL THEN 'PRESENTE'
            WHEN x.TipoEsperado='FUNCTION' AND f.object_id IS NOT NULL THEN 'PRESENTE'
            ELSE 'FALTANTE' END AS Estado
FROM @Objetos AS x
LEFT JOIN sys.tables AS t ON t.name=x.Nombre
LEFT JOIN sys.procedures AS p ON p.name=x.Nombre
LEFT JOIN sys.objects AS f ON f.name=x.Nombre AND f.type IN ('FN','IF','TF')
ORDER BY x.TipoEsperado,x.Nombre;

SELECT OBJECT_SCHEMA_NAME(c.object_id) AS Esquema, OBJECT_NAME(c.object_id) AS Objeto,
       c.name AS Columna, TYPE_NAME(c.user_type_id) AS Tipo, c.max_length AS LongitudBytes,
       c.precision,c.scale,c.is_nullable
FROM sys.columns AS c
WHERE OBJECT_NAME(c.object_id) IN ('PRODUCTO','CATEGORIA','MARCA','CARRITO','VENTA','DETALLE_VENTA',
 'CLIENTE','USUARIO','DEPARTAMENTO','MUNICIPIO','CIUDAD')
ORDER BY Objeto,c.column_id;

/* Organiza sección */
SELECT s.name AS Esquema, tt.name AS TipoTabla
FROM sys.table_types AS tt JOIN sys.schemas AS s ON s.schema_id=tt.schema_id
ORDER BY s.name,tt.name;
