/*
    Provisiona el esquema vacio de LumiStore usando la estructura verificada
    en la base local DBCARRITO. No copia datos de clientes, usuarios ni ventas.
    Ejecutar con la base lumistore seleccionada.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'lumistore'
    THROW 51000, 'Base incorrecta: seleccione lumistore antes de ejecutar.', 1;

IF EXISTS (SELECT 1 FROM sys.tables WHERE is_ms_shipped = 0)
    THROW 51001, 'La base ya contiene tablas de usuario; el script se cancela sin cambios.', 1;

BEGIN TRANSACTION;

CREATE TABLE dbo.DEPARTAMENTO
(
    IdDepartamento varchar(2) NOT NULL CONSTRAINT PK_DEPARTAMENTO PRIMARY KEY,
    Descripcion varchar(45) NOT NULL
);

CREATE TABLE dbo.MUNICIPIO
(
    IdMunicipio varchar(4) NOT NULL CONSTRAINT PK_MUNICIPIO PRIMARY KEY,
    Descripcion varchar(45) NOT NULL,
    IdDepartamento varchar(2) NOT NULL
);

CREATE TABLE dbo.CIUDAD
(
    IdCiudad varchar(6) NOT NULL CONSTRAINT PK_CIUDAD PRIMARY KEY,
    Descripcion varchar(45) NOT NULL,
    IdMunicipio varchar(4) NOT NULL,
    IdDepartamento varchar(2) NOT NULL
);

CREATE TABLE dbo.CATEGORIA
(
    IdCategoria int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CATEGORIA PRIMARY KEY,
    Descripcion varchar(100) NULL,
    Activo bit NULL,
    FechaRegistro datetime NULL
);

CREATE TABLE dbo.MARCA
(
    IdMarca int IDENTITY(1,1) NOT NULL CONSTRAINT PK_MARCA PRIMARY KEY,
    Descripcion varchar(100) NULL,
    Activo bit NULL,
    FechaRegistro datetime NULL
);

CREATE TABLE dbo.PRODUCTO
(
    IdProducto int IDENTITY(1,1) NOT NULL CONSTRAINT PK_PRODUCTO PRIMARY KEY,
    Nombre varchar(500) NULL,
    Descripcion varchar(500) NULL,
    IdMarca int NULL,
    IdCategoria int NULL,
    Precio decimal(18,2) NULL,
    Stock int NULL,
    RutaImagen varchar(100) NULL,
    NombreImagen varchar(100) NULL,
    Activo bit NULL,
    FechaRegistro datetime NULL
);

CREATE TABLE dbo.CLIENTE
(
    IdCliente int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CLIENTE PRIMARY KEY,
    Nombres varchar(100) NULL,
    Apellidos varchar(100) NULL,
    Correo varchar(100) NULL,
    Clave varchar(150) NULL,
    Reestablecer bit NULL,
    FechaRegistro datetime NULL
);

CREATE TABLE dbo.USUARIO
(
    IdUsuario int IDENTITY(1,1) NOT NULL CONSTRAINT PK_USUARIO PRIMARY KEY,
    Nombres varchar(100) NULL,
    Apellidos varchar(100) NULL,
    Correo varchar(100) NULL,
    Clave varchar(150) NULL,
    Reestablecer bit NULL,
    Activo bit NULL,
    FechaRegistro datetime NULL
);

CREATE TABLE dbo.CARRITO
(
    IdCarrito int IDENTITY(1,1) NOT NULL CONSTRAINT PK_CARRITO PRIMARY KEY,
    IdCliente int NULL,
    IdProducto int NULL,
    Cantidad int NULL
);

CREATE TABLE dbo.VENTA
(
    IdVenta int IDENTITY(1,1) NOT NULL CONSTRAINT PK_VENTA PRIMARY KEY,
    IdCliente int NULL,
    TotalProducto int NULL,
    MontoTotal decimal(18,2) NULL,
    Contacto varchar(50) NULL,
    IdCiudad varchar(10) NULL,
    Telefono varchar(50) NULL,
    Direccion varchar(500) NULL,
    IdTransaccion varchar(50) NULL,
    FechaVenta datetime NULL
);

CREATE TABLE dbo.DETALLE_VENTA
(
    IdDetalleVenta int IDENTITY(1,1) NOT NULL CONSTRAINT PK_DETALLE_VENTA PRIMARY KEY,
    IdVenta int NULL,
    IdProducto int NULL,
    Cantidad int NULL,
    Total decimal(18,2) NULL
);

ALTER TABLE dbo.MUNICIPIO WITH CHECK ADD CONSTRAINT FK_MUNICIPIO_DEPARTAMENTO
    FOREIGN KEY (IdDepartamento) REFERENCES dbo.DEPARTAMENTO (IdDepartamento);
ALTER TABLE dbo.CIUDAD WITH CHECK ADD CONSTRAINT FK_CIUDAD_MUNICIPIO
    FOREIGN KEY (IdMunicipio) REFERENCES dbo.MUNICIPIO (IdMunicipio);
ALTER TABLE dbo.CIUDAD WITH CHECK ADD CONSTRAINT FK_CIUDAD_DEPARTAMENTO
    FOREIGN KEY (IdDepartamento) REFERENCES dbo.DEPARTAMENTO (IdDepartamento);
ALTER TABLE dbo.PRODUCTO WITH CHECK ADD CONSTRAINT FK_PRODUCTO_MARCA
    FOREIGN KEY (IdMarca) REFERENCES dbo.MARCA (IdMarca);
ALTER TABLE dbo.PRODUCTO WITH CHECK ADD CONSTRAINT FK_PRODUCTO_CATEGORIA
    FOREIGN KEY (IdCategoria) REFERENCES dbo.CATEGORIA (IdCategoria);
ALTER TABLE dbo.CARRITO WITH CHECK ADD CONSTRAINT FK_CARRITO_CLIENTE
    FOREIGN KEY (IdCliente) REFERENCES dbo.CLIENTE (IdCliente);
ALTER TABLE dbo.CARRITO WITH CHECK ADD CONSTRAINT FK_CARRITO_PRODUCTO
    FOREIGN KEY (IdProducto) REFERENCES dbo.PRODUCTO (IdProducto);
ALTER TABLE dbo.VENTA WITH CHECK ADD CONSTRAINT FK_VENTA_CLIENTE
    FOREIGN KEY (IdCliente) REFERENCES dbo.CLIENTE (IdCliente);
ALTER TABLE dbo.DETALLE_VENTA WITH CHECK ADD CONSTRAINT FK_DETALLE_VENTA_VENTA
    FOREIGN KEY (IdVenta) REFERENCES dbo.VENTA (IdVenta);
ALTER TABLE dbo.DETALLE_VENTA WITH CHECK ADD CONSTRAINT FK_DETALLE_VENTA_PRODUCTO
    FOREIGN KEY (IdProducto) REFERENCES dbo.PRODUCTO (IdProducto);

COMMIT TRANSACTION;
GO

CREATE OR ALTER FUNCTION dbo.fn_obtenerCarritoCliente(@idcliente int)
RETURNS TABLE
AS
RETURN
(
    SELECT p.IdProducto, m.Descripcion AS DesMarca, p.Nombre, p.Precio,
           c.Cantidad, p.RutaImagen, p.NombreImagen
    FROM dbo.CARRITO AS c
    INNER JOIN dbo.PRODUCTO AS p ON p.IdProducto = c.IdProducto
    INNER JOIN dbo.MARCA AS m ON m.IdMarca = p.IdMarca
    WHERE c.IdCliente = @idcliente
);
GO
