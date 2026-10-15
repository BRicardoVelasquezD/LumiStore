/*
  LumiStore — claves y relaciones verificadas de DBCARRITO.

  DDL autorizado expresamente por el usuario para la base local DBCARRITO.
  No inserta, actualiza ni elimina filas de negocio. Valida las restricciones
  existentes mediante WITH CHECK y realiza todo el DDL en una transacción:
  ante cualquier error, se revierte el conjunto completo.

  Preflight SELECT ejecutado antes de este script: 0 grupos duplicados y 0
  nulos en las tres claves geográficas; 0 huérfanos en todas las relaciones
  incluidas. Las tablas pueden haber cambiado desde el preflight; WITH CHECK
  vuelve a validarlas y aborta/rollback si ahora hay datos incompatibles.

  Se omite VENTA.IdCiudad -> CIUDAD.IdCiudad: son varchar(10) y varchar(6),
  respectivamente. Aunque el preflight no encontró huérfanos, no se alteran
  longitudes ni se declara esa relación sin validar el contrato de columna.

  Ejecutar solo contra (localdb)\MSSQLLocalDB / DBCARRITO:
	sqlcmd -S "(localdb)\MSSQLLocalDB" -d "DBCARRITO" -E -b -i "docs\sql\relaciones_seguidas_dbcarrito.sql"
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'DBCARRITO'
BEGIN
	RAISERROR(N'Abortado: la base actual no es DBCARRITO.', 16, 1);
	RETURN;
END;

BEGIN TRY
	BEGIN TRANSACTION;

	IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.CIUDAD') AND type = 'PK')
		ALTER TABLE dbo.CIUDAD ADD CONSTRAINT PK_CIUDAD PRIMARY KEY (IdCiudad);

	IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.DEPARTAMENTO') AND type = 'PK')
		ALTER TABLE dbo.DEPARTAMENTO ADD CONSTRAINT PK_DEPARTAMENTO PRIMARY KEY (IdDepartamento);

	IF NOT EXISTS (SELECT 1 FROM sys.key_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.MUNICIPIO') AND type = 'PK')
		ALTER TABLE dbo.MUNICIPIO ADD CONSTRAINT PK_MUNICIPIO PRIMARY KEY (IdMunicipio);

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CARRITO_CLIENTE')
		ALTER TABLE dbo.CARRITO WITH CHECK ADD CONSTRAINT FK_CARRITO_CLIENTE
			FOREIGN KEY (IdCliente) REFERENCES dbo.CLIENTE (IdCliente) ON DELETE NO ACTION ON UPDATE NO ACTION;

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CARRITO_PRODUCTO')
		ALTER TABLE dbo.CARRITO WITH CHECK ADD CONSTRAINT FK_CARRITO_PRODUCTO
			FOREIGN KEY (IdProducto) REFERENCES dbo.PRODUCTO (IdProducto) ON DELETE NO ACTION ON UPDATE NO ACTION;

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PRODUCTO_MARCA')
		ALTER TABLE dbo.PRODUCTO WITH CHECK ADD CONSTRAINT FK_PRODUCTO_MARCA
			FOREIGN KEY (IdMarca) REFERENCES dbo.MARCA (IdMarca) ON DELETE NO ACTION ON UPDATE NO ACTION;

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PRODUCTO_CATEGORIA')
		ALTER TABLE dbo.PRODUCTO WITH CHECK ADD CONSTRAINT FK_PRODUCTO_CATEGORIA
			FOREIGN KEY (IdCategoria) REFERENCES dbo.CATEGORIA (IdCategoria) ON DELETE NO ACTION ON UPDATE NO ACTION;

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DETALLE_VENTA_VENTA')
		ALTER TABLE dbo.DETALLE_VENTA WITH CHECK ADD CONSTRAINT FK_DETALLE_VENTA_VENTA
			FOREIGN KEY (IdVenta) REFERENCES dbo.VENTA (IdVenta) ON DELETE NO ACTION ON UPDATE NO ACTION;

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_DETALLE_VENTA_PRODUCTO')
		ALTER TABLE dbo.DETALLE_VENTA WITH CHECK ADD CONSTRAINT FK_DETALLE_VENTA_PRODUCTO
			FOREIGN KEY (IdProducto) REFERENCES dbo.PRODUCTO (IdProducto) ON DELETE NO ACTION ON UPDATE NO ACTION;

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_VENTA_CLIENTE')
		ALTER TABLE dbo.VENTA WITH CHECK ADD CONSTRAINT FK_VENTA_CLIENTE
			FOREIGN KEY (IdCliente) REFERENCES dbo.CLIENTE (IdCliente) ON DELETE NO ACTION ON UPDATE NO ACTION;

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_MUNICIPIO_DEPARTAMENTO')
		ALTER TABLE dbo.MUNICIPIO WITH CHECK ADD CONSTRAINT FK_MUNICIPIO_DEPARTAMENTO
			FOREIGN KEY (IdDepartamento) REFERENCES dbo.DEPARTAMENTO (IdDepartamento) ON DELETE NO ACTION ON UPDATE NO ACTION;

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CIUDAD_MUNICIPIO')
		ALTER TABLE dbo.CIUDAD WITH CHECK ADD CONSTRAINT FK_CIUDAD_MUNICIPIO
			FOREIGN KEY (IdMunicipio) REFERENCES dbo.MUNICIPIO (IdMunicipio) ON DELETE NO ACTION ON UPDATE NO ACTION;

	IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CIUDAD_DEPARTAMENTO')
		ALTER TABLE dbo.CIUDAD WITH CHECK ADD CONSTRAINT FK_CIUDAD_DEPARTAMENTO
			FOREIGN KEY (IdDepartamento) REFERENCES dbo.DEPARTAMENTO (IdDepartamento) ON DELETE NO ACTION ON UPDATE NO ACTION;

	COMMIT TRANSACTION;
END TRY
BEGIN CATCH
	IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
	THROW;
END CATCH;
