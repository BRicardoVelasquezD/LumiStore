/*
	LumiStore — diagnóstico de catálogo de DBCARRITO
	Alcance: SOLO LECTURA. No consulta filas de negocio ni ejecuta CRUD.

	Conexión local Windows Authentication (PowerShell):
	  sqlcmd -S "(localdb)\MSSQLLocalDB" -d "DBCARRITO" -E -b -i "docs\sql\diagnostico_catalogo_dbcarrito.sql"

	Cada bloque consulta metadatos sys.* / INFORMATION_SCHEMA. No incluye INSERT,
	UPDATE, DELETE, MERGE, EXEC, DDL, transacciones de escritura ni migraciones.
	El bloque de definiciones devuelve el texto de procedimientos, funciones,
	vistas y triggers para contrastarlo manualmente con el código de Datos/ y
	Negocio/. Este script no crea objetos y no demuestra que las rutas CRUD
	funcionen. Ejecutar solo en DBCARRITO; si el contexto es otra base, aborta.
*/
SET NOCOUNT ON;

IF DB_NAME() <> N'DBCARRITO'
BEGIN
	RAISERROR(N'Abortado: la base actual no es DBCARRITO. No se ha modificado ningún objeto.', 16, 1);
	RETURN;
END;

-- 1. Identidad de la conexión y resumen de objetos de usuario (sin datos de negocio).
SELECT DB_NAME() AS BaseActual, @@SERVERNAME AS ServidorActual;

SELECT o.type AS CodigoTipo,
	   o.type_desc AS TipoObjeto,
	   COUNT_BIG(*) AS Cantidad
FROM sys.objects AS o
WHERE o.is_ms_shipped = 0
GROUP BY o.type, o.type_desc
ORDER BY o.type_desc;

-- 2. Tablas y columnas: tipo SQL, nulabilidad, identidad y computed.
SELECT s.name AS Esquema,
	   t.name AS Tabla,
	   c.column_id AS OrdenColumna,
	   c.name AS Columna,
	   ty.name AS TipoSQL,
	   CASE WHEN ty.name IN (N'nvarchar', N'nchar', N'ntext') AND c.max_length > 0
			THEN c.max_length / 2 ELSE c.max_length END AS LongitudBytesOCaracteres,
	   c.precision AS PrecisionSQL,
	   c.scale AS EscalaSQL,
	   c.is_nullable AS PermiteNull,
	   c.is_identity AS EsIdentity,
	   c.is_computed AS EsCalculada
FROM sys.tables AS t
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
JOIN sys.columns AS c ON c.object_id = t.object_id
JOIN sys.types AS ty ON ty.user_type_id = c.user_type_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name, c.column_id;

-- 3. Claves primarias y únicas, con el orden de sus columnas.
SELECT s.name AS Esquema,
	   t.name AS Tabla,
	   kc.name AS Restriccion,
	   kc.type_desc AS TipoRestriccion,
	   ic.key_ordinal AS OrdenClave,
	   c.name AS Columna,
	   i.is_unique AS EsUnica,
	   i.is_primary_key AS EsPrimaria
FROM sys.key_constraints AS kc
JOIN sys.tables AS t ON t.object_id = kc.parent_object_id
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
JOIN sys.indexes AS i ON i.object_id = kc.parent_object_id AND i.index_id = kc.unique_index_id
JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal > 0
JOIN sys.columns AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
ORDER BY s.name, t.name, kc.type_desc, kc.name, ic.key_ordinal;

-- 4. Relaciones declaradas mediante claves foráneas, incluidos sus extremos.
SELECT fk.name AS ClaveForanea,
	   ps.name AS EsquemaOrigen,
	   pt.name AS TablaOrigen,
	   pc.name AS ColumnaOrigen,
	   rs.name AS EsquemaDestino,
	   rt.name AS TablaDestino,
	   rc.name AS ColumnaDestino,
	   fkc.constraint_column_id AS OrdenColumna,
	   fk.is_disabled AS EstaDeshabilitada,
	   fk.delete_referential_action_desc AS AccionAlEliminar,
	   fk.update_referential_action_desc AS AccionAlActualizar
FROM sys.foreign_keys AS fk
JOIN sys.foreign_key_columns AS fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.tables AS pt ON pt.object_id = fkc.parent_object_id
JOIN sys.schemas AS ps ON ps.schema_id = pt.schema_id
JOIN sys.columns AS pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id
JOIN sys.tables AS rt ON rt.object_id = fkc.referenced_object_id
JOIN sys.schemas AS rs ON rs.schema_id = rt.schema_id
JOIN sys.columns AS rc ON rc.object_id = fkc.referenced_object_id AND rc.column_id = fkc.referenced_column_id
ORDER BY ps.name, pt.name, fk.name, fkc.constraint_column_id;

-- 5. Índices y restricciones por columna, para completar la inspección de claves.
SELECT s.name AS Esquema,
	   t.name AS Tabla,
	   i.name AS Indice,
	   i.type_desc AS TipoIndice,
	   i.is_unique AS EsUnico,
	   i.is_primary_key AS EsClavePrimaria,
	   i.is_unique_constraint AS EsRestriccionUnica,
	   ic.key_ordinal AS OrdenClave,
	   ic.is_included_column AS EsColumnaIncluida,
	   c.name AS Columna
FROM sys.indexes AS i
JOIN sys.tables AS t ON t.object_id = i.object_id
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE i.index_id > 0 AND i.is_hypothetical = 0
ORDER BY s.name, t.name, i.name, ic.key_ordinal, ic.index_column_id;

-- 6. Restricciones DEFAULT y CHECK definidas en tablas.
SELECT s.name AS Esquema,
	   t.name AS Tabla,
	   dc.name AS RestriccionDefault,
	   c.name AS Columna,
	   dc.definition AS ExpresionDefault,
	   dc.is_system_named AS NombreGeneradoPorSistema
FROM sys.default_constraints AS dc
JOIN sys.tables AS t ON t.object_id = dc.parent_object_id
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
JOIN sys.columns AS c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
ORDER BY s.name, t.name, c.column_id;

SELECT s.name AS Esquema,
	   t.name AS Tabla,
	   cc.name AS RestriccionCheck,
	   cc.definition AS ExpresionCheck,
	   cc.is_disabled AS EstaDeshabilitada,
	   cc.is_not_trusted AS NoConfiable
FROM sys.check_constraints AS cc
JOIN sys.tables AS t ON t.object_id = cc.parent_object_id
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
ORDER BY s.name, t.name, cc.name;

-- 7. Procedimientos, funciones y vistas de usuario, parámetros y definiciones.
SELECT s.name AS Esquema,
	   o.name AS Objeto,
	   o.type_desc AS TipoObjeto,
	   p.parameter_id AS OrdenParametro,
	   p.name AS Parametro,
	   ty.name AS TipoParametro,
	   p.max_length AS LongitudBytes,
	   p.precision AS PrecisionSQL,
	   p.scale AS EscalaSQL,
	   p.is_output AS EsSalida,
	   p.is_readonly AS EsSoloLectura,
	   OBJECT_DEFINITION(o.object_id) AS DefinicionSQL
FROM sys.objects AS o
JOIN sys.schemas AS s ON s.schema_id = o.schema_id
LEFT JOIN sys.parameters AS p ON p.object_id = o.object_id
LEFT JOIN sys.types AS ty ON ty.user_type_id = p.user_type_id
WHERE o.is_ms_shipped = 0
  AND o.type IN ('P', 'FN', 'IF', 'TF', 'FS', 'FT', 'V')
ORDER BY o.type_desc, s.name, o.name, p.parameter_id;

-- 8. Tipos de tabla definidos por el usuario (relevante para parámetros TVP).
SELECT s.name AS Esquema,
	   tt.name AS TipoTabla,
	   c.column_id AS OrdenColumna,
	   c.name AS Columna,
	   ty.name AS TipoSQL,
	   c.max_length AS LongitudBytes,
	   c.precision AS PrecisionSQL,
	   c.scale AS EscalaSQL,
	   c.is_nullable AS PermiteNull
FROM sys.table_types AS tt
JOIN sys.schemas AS s ON s.schema_id = tt.schema_id
JOIN sys.columns AS c ON c.object_id = tt.type_table_object_id
JOIN sys.types AS ty ON ty.user_type_id = c.user_type_id
ORDER BY s.name, tt.name, c.column_id;

-- 9. Triggers DML/DDL de usuario y definiciones (no se ejecutan).
SELECT s.name AS Esquema,
	   o.name AS ObjetoPadre,
		  tr.name AS NombreTrigger,
	   tr.parent_class_desc AS ClasePadre,
	   tr.is_disabled AS EstaDeshabilitado,
	   tr.is_instead_of_trigger AS EsInsteadOf,
	   OBJECT_DEFINITION(tr.object_id) AS DefinicionSQL
FROM sys.triggers AS tr
LEFT JOIN sys.objects AS o ON o.object_id = tr.parent_id
LEFT JOIN sys.schemas AS s ON s.schema_id = o.schema_id
WHERE tr.is_ms_shipped = 0
ORDER BY s.name, o.name, tr.name;