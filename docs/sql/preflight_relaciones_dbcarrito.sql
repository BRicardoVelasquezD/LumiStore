/*
  Preflight de integridad para posibles PK/FK de DBCARRITO.
  SOLO LECTURA: devuelve únicamente recuentos agregados; no muestra valores
  de filas y no cambia datos ni esquema. Ejecutar con -d DBCARRITO.
*/
SET NOCOUNT ON;
IF DB_NAME() <> N'DBCARRITO'
BEGIN
	RAISERROR(N'Abortado: la base actual no es DBCARRITO.', 16, 1);
	RETURN;
END;

-- Unicidad y nulos en posibles claves de ubicación.
SELECT N'CIUDAD.IdCiudad: grupos duplicados' AS Comprobacion, COUNT_BIG(*) AS Hallazgos
FROM (SELECT IdCiudad FROM dbo.CIUDAD GROUP BY IdCiudad HAVING COUNT_BIG(*) > 1) AS d
UNION ALL
SELECT N'DEPARTAMENTO.IdDepartamento: grupos duplicados', COUNT_BIG(*)
FROM (SELECT IdDepartamento FROM dbo.DEPARTAMENTO GROUP BY IdDepartamento HAVING COUNT_BIG(*) > 1) AS d
UNION ALL
SELECT N'MUNICIPIO.IdMunicipio: grupos duplicados', COUNT_BIG(*)
FROM (SELECT IdMunicipio FROM dbo.MUNICIPIO GROUP BY IdMunicipio HAVING COUNT_BIG(*) > 1) AS d
UNION ALL SELECT N'CIUDAD.IdCiudad: nulos', COUNT_BIG(*) FROM dbo.CIUDAD WHERE IdCiudad IS NULL
UNION ALL SELECT N'DEPARTAMENTO.IdDepartamento: nulos', COUNT_BIG(*) FROM dbo.DEPARTAMENTO WHERE IdDepartamento IS NULL
UNION ALL SELECT N'MUNICIPIO.IdMunicipio: nulos', COUNT_BIG(*) FROM dbo.MUNICIPIO WHERE IdMunicipio IS NULL;

-- Filas huérfanas en relaciones inferidas de las consultas y los nombres de columnas.
SELECT N'CARRITO.IdCliente -> CLIENTE.IdCliente' AS Relacion, COUNT_BIG(*) AS Huerfanos
FROM dbo.CARRITO AS c LEFT JOIN dbo.CLIENTE AS p ON p.IdCliente = c.IdCliente
WHERE c.IdCliente IS NOT NULL AND p.IdCliente IS NULL
UNION ALL SELECT N'CARRITO.IdProducto -> PRODUCTO.IdProducto', COUNT_BIG(*)
FROM dbo.CARRITO AS c LEFT JOIN dbo.PRODUCTO AS p ON p.IdProducto = c.IdProducto
WHERE c.IdProducto IS NOT NULL AND p.IdProducto IS NULL
UNION ALL SELECT N'PRODUCTO.IdMarca -> MARCA.IdMarca', COUNT_BIG(*)
FROM dbo.PRODUCTO AS c LEFT JOIN dbo.MARCA AS p ON p.IdMarca = c.IdMarca
WHERE c.IdMarca IS NOT NULL AND p.IdMarca IS NULL
UNION ALL SELECT N'PRODUCTO.IdCategoria -> CATEGORIA.IdCategoria', COUNT_BIG(*)
FROM dbo.PRODUCTO AS c LEFT JOIN dbo.CATEGORIA AS p ON p.IdCategoria = c.IdCategoria
WHERE c.IdCategoria IS NOT NULL AND p.IdCategoria IS NULL
UNION ALL SELECT N'DETALLE_VENTA.IdVenta -> VENTA.IdVenta', COUNT_BIG(*)
FROM dbo.DETALLE_VENTA AS c LEFT JOIN dbo.VENTA AS p ON p.IdVenta = c.IdVenta
WHERE c.IdVenta IS NOT NULL AND p.IdVenta IS NULL
UNION ALL SELECT N'DETALLE_VENTA.IdProducto -> PRODUCTO.IdProducto', COUNT_BIG(*)
FROM dbo.DETALLE_VENTA AS c LEFT JOIN dbo.PRODUCTO AS p ON p.IdProducto = c.IdProducto
WHERE c.IdProducto IS NOT NULL AND p.IdProducto IS NULL
UNION ALL SELECT N'VENTA.IdCliente -> CLIENTE.IdCliente', COUNT_BIG(*)
FROM dbo.VENTA AS c LEFT JOIN dbo.CLIENTE AS p ON p.IdCliente = c.IdCliente
WHERE c.IdCliente IS NOT NULL AND p.IdCliente IS NULL
UNION ALL SELECT N'VENTA.IdCiudad -> CIUDAD.IdCiudad', COUNT_BIG(*)
FROM dbo.VENTA AS c LEFT JOIN dbo.CIUDAD AS p ON p.IdCiudad = c.IdCiudad
WHERE c.IdCiudad IS NOT NULL AND p.IdCiudad IS NULL
UNION ALL SELECT N'MUNICIPIO.IdDepartamento -> DEPARTAMENTO.IdDepartamento', COUNT_BIG(*)
FROM dbo.MUNICIPIO AS c LEFT JOIN dbo.DEPARTAMENTO AS p ON p.IdDepartamento = c.IdDepartamento
WHERE c.IdDepartamento IS NOT NULL AND p.IdDepartamento IS NULL
UNION ALL SELECT N'CIUDAD.IdMunicipio -> MUNICIPIO.IdMunicipio', COUNT_BIG(*)
FROM dbo.CIUDAD AS c LEFT JOIN dbo.MUNICIPIO AS p ON p.IdMunicipio = c.IdMunicipio
WHERE c.IdMunicipio IS NOT NULL AND p.IdMunicipio IS NULL
UNION ALL SELECT N'CIUDAD.IdDepartamento -> DEPARTAMENTO.IdDepartamento', COUNT_BIG(*)
FROM dbo.CIUDAD AS c LEFT JOIN dbo.DEPARTAMENTO AS p ON p.IdDepartamento = c.IdDepartamento
WHERE c.IdDepartamento IS NOT NULL AND p.IdDepartamento IS NULL;
