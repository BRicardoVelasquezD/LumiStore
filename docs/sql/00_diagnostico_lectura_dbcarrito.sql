/* Organiza sección */
SET NOCOUNT ON;

SELECT DB_NAME() AS BaseActual, SUSER_SNAME() AS LoginActual;

SELECT s.name AS Esquema, o.name AS Objeto, o.type_desc AS Tipo, o.create_date AS Creado, o.modify_date AS Modificado
FROM sys.objects AS o
JOIN sys.schemas AS s ON s.schema_id = o.schema_id
WHERE o.type IN ('U','P','FN','IF','TF','V')
ORDER BY o.type_desc, s.name, o.name;

SELECT s.name AS Esquema, t.name AS Tabla, c.column_id AS Orden, c.name AS Columna,
       ty.name AS Tipo, c.max_length AS LongitudBytes, c.precision AS Precision,
       c.scale AS Escala, c.is_nullable AS PermiteNull, c.is_identity AS EsIdentity,
       dc.definition AS ValorPredeterminado
FROM sys.tables AS t
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
JOIN sys.columns AS c ON c.object_id = t.object_id
JOIN sys.types AS ty ON ty.user_type_id = c.user_type_id
LEFT JOIN sys.default_constraints AS dc ON dc.object_id = c.default_object_id
ORDER BY s.name, t.name, c.column_id;

SELECT s.name AS Esquema, p.name AS Procedimiento,
       prm.parameter_id AS Orden, prm.name AS Parametro, TYPE_NAME(prm.user_type_id) AS Tipo,
       prm.max_length AS LongitudBytes, prm.precision AS Precision, prm.scale AS Escala,
       prm.is_output AS EsSalida, prm.is_readonly AS EsSoloLectura
FROM sys.procedures AS p
JOIN sys.schemas AS s ON s.schema_id = p.schema_id
LEFT JOIN sys.parameters AS prm ON prm.object_id = p.object_id
ORDER BY s.name, p.name, prm.parameter_id;

SELECT s.name AS Esquema, tt.name AS TipoTabla, c.column_id AS Orden, c.name AS Columna,
       TYPE_NAME(c.user_type_id) AS Tipo, c.max_length AS LongitudBytes, c.precision, c.scale, c.is_nullable
FROM sys.table_types AS tt
JOIN sys.schemas AS s ON s.schema_id = tt.schema_id
JOIN sys.columns AS c ON c.object_id = tt.type_table_object_id
ORDER BY s.name, tt.name, c.column_id;

SELECT s.name AS Esquema, t.name AS Tabla, i.name AS Indice, i.is_primary_key AS EsPK,
       i.is_unique AS EsUnico, c.name AS Columna, ic.key_ordinal AS OrdenClave
FROM sys.indexes AS i
JOIN sys.tables AS t ON t.object_id = i.object_id
JOIN sys.schemas AS s ON s.schema_id = t.schema_id
JOIN sys.index_columns AS ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
JOIN sys.columns AS c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE i.name IS NOT NULL
ORDER BY s.name, t.name, i.name, ic.key_ordinal;

SELECT fk.name AS ClaveForanea, OBJECT_SCHEMA_NAME(fk.parent_object_id) AS EsquemaOrigen,
       OBJECT_NAME(fk.parent_object_id) AS TablaOrigen, pc.name AS ColumnaOrigen,
       OBJECT_SCHEMA_NAME(fk.referenced_object_id) AS EsquemaDestino,
       OBJECT_NAME(fk.referenced_object_id) AS TablaDestino, rc.name AS ColumnaDestino,
       fk.is_disabled AS Deshabilitada, fk.is_not_trusted AS NoConfiable
FROM sys.foreign_keys AS fk
JOIN sys.foreign_key_columns AS fkc ON fkc.constraint_object_id = fk.object_id
JOIN sys.columns AS pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id
JOIN sys.columns AS rc ON rc.object_id = fkc.referenced_object_id AND rc.column_id = fkc.referenced_column_id
ORDER BY fk.name, fkc.constraint_column_id;

SELECT s.name AS Esquema, o.name AS Objeto, d.referenced_schema_name AS EsquemaReferido,
       d.referenced_entity_name AS ObjetoReferido, d.referenced_minor_id AS IdColumnaReferida
FROM sys.sql_expression_dependencies AS d
JOIN sys.objects AS o ON o.object_id = d.referencing_id
JOIN sys.schemas AS s ON s.schema_id = o.schema_id
ORDER BY s.name, o.name, d.referenced_schema_name, d.referenced_entity_name;
