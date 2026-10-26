/*
    Contratos usados por la capa Datos. Requiere primero
    07_provision_lumistore_schema.sql y la base lumistore.
*/
SET NOCOUNT ON;
IF DB_NAME() <> N'lumistore'
    THROW 51000, 'Base incorrecta: seleccione lumistore antes de ejecutar.', 1;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ExisteCarrito
    @IdCliente int,
    @IdProducto int,
    @Resultado bit OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = CASE WHEN EXISTS
        (SELECT 1 FROM dbo.CARRITO WHERE IdCliente = @IdCliente AND IdProducto = @IdProducto)
        THEN 1 ELSE 0 END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_OperacionCarrito
    @IdCliente int,
    @IdProducto int,
    @Sumar bit,
    @Resultado bit OUTPUT,
    @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    SET @Resultado = 0;
    SET @Mensaje = '';

    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @Stock int, @Activo bit, @Cantidad int;
        SELECT @Stock = Stock, @Activo = Activo
        FROM dbo.PRODUCTO WITH (UPDLOCK, HOLDLOCK)
        WHERE IdProducto = @IdProducto;

        IF @Stock IS NULL OR ISNULL(@Activo, 0) = 0
            THROW 51001, 'El producto no está disponible.', 1;

        SELECT TOP (1) @Cantidad = Cantidad
        FROM dbo.CARRITO WITH (UPDLOCK, HOLDLOCK)
        WHERE IdCliente = @IdCliente AND IdProducto = @IdProducto
        ORDER BY IdCarrito;

        IF @Sumar = 1
        BEGIN
            IF ISNULL(@Cantidad, 0) >= @Stock
                THROW 51002, 'No hay unidades disponibles.', 1;

            IF @Cantidad IS NULL
                INSERT dbo.CARRITO (IdCliente, IdProducto, Cantidad) VALUES (@IdCliente, @IdProducto, 1);
            ELSE
                UPDATE dbo.CARRITO SET Cantidad = Cantidad + 1
                WHERE IdCliente = @IdCliente AND IdProducto = @IdProducto;
        END
        ELSE
        BEGIN
            IF @Cantidad IS NULL
                THROW 51003, 'El producto no está en el carrito.', 1;
            IF @Cantidad <= 1
                DELETE dbo.CARRITO WHERE IdCliente = @IdCliente AND IdProducto = @IdProducto;
            ELSE
                UPDATE dbo.CARRITO SET Cantidad = Cantidad - 1
                WHERE IdCliente = @IdCliente AND IdProducto = @IdProducto;
        END

        COMMIT TRANSACTION;
        SET @Resultado = 1;
        SET @Mensaje = 'Carrito actualizado.';
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        SET @Mensaje = LEFT(ERROR_MESSAGE(), 500);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_EliminarCarrito
    @IdCliente int,
    @IdProducto int,
    @Resultado bit OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE dbo.CARRITO WHERE IdCliente = @IdCliente AND IdProducto = @IdProducto;
    SET @Resultado = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RegistrarCategoria
    @Descripcion varchar(100), @Activo bit,
    @Resultado int OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT dbo.CATEGORIA (Descripcion, Activo, FechaRegistro)
        VALUES (@Descripcion, @Activo, GETDATE());
        SET @Resultado = CONVERT(int, SCOPE_IDENTITY());
        SET @Mensaje = 'Categoría creada.';
    END TRY
    BEGIN CATCH
        SET @Resultado = 0; SET @Mensaje = LEFT(ERROR_MESSAGE(), 500);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_EditarCategoria
    @IdCategoria int, @Descripcion varchar(100), @Activo bit,
    @Resultado bit OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.CATEGORIA SET Descripcion = @Descripcion, Activo = @Activo WHERE IdCategoria = @IdCategoria;
    SET @Resultado = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
    SET @Mensaje = CASE WHEN @Resultado = 1 THEN 'Categoría actualizada.' ELSE 'Categoría no encontrada.' END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_EliminarCategoria
    @IdCategoria int, @Resultado bit OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE dbo.CATEGORIA WHERE IdCategoria = @IdCategoria;
        SET @Resultado = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
        SET @Mensaje = CASE WHEN @Resultado = 1 THEN 'Categoría eliminada.' ELSE 'Categoría no encontrada.' END;
    END TRY
    BEGIN CATCH
        SET @Resultado = 0; SET @Mensaje = LEFT(ERROR_MESSAGE(), 500);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RegistrarCliente
    @Nombres varchar(100), @Apellidos varchar(100), @Correo varchar(100), @Clave varchar(150),
    @Resultado int OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    IF EXISTS (SELECT 1 FROM dbo.CLIENTE WHERE Correo = @Correo)
    BEGIN SET @Mensaje = 'Ya existe una cuenta con ese correo.'; RETURN; END;
    BEGIN TRY
        INSERT dbo.CLIENTE (Nombres, Apellidos, Correo, Clave, Reestablecer, FechaRegistro)
        VALUES (@Nombres, @Apellidos, @Correo, @Clave, 0, GETDATE());
        SET @Resultado = CONVERT(int, SCOPE_IDENTITY());
        SET @Mensaje = 'Cliente registrado.';
    END TRY
    BEGIN CATCH
        SET @Resultado = 0; SET @Mensaje = LEFT(ERROR_MESSAGE(), 500);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RegistrarMarca
    @Descripcion varchar(100), @Activo bit,
    @Resultado int OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT dbo.MARCA (Descripcion, Activo, FechaRegistro) VALUES (@Descripcion, @Activo, GETDATE());
        SET @Resultado = CONVERT(int, SCOPE_IDENTITY());
        SET @Mensaje = 'Marca creada.';
    END TRY
    BEGIN CATCH
        SET @Resultado = 0; SET @Mensaje = LEFT(ERROR_MESSAGE(), 500);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_EditarMarca
    @IdMarca int, @Descripcion varchar(100), @Activo bit,
    @Resultado bit OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.MARCA SET Descripcion = @Descripcion, Activo = @Activo WHERE IdMarca = @IdMarca;
    SET @Resultado = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
    SET @Mensaje = CASE WHEN @Resultado = 1 THEN 'Marca actualizada.' ELSE 'Marca no encontrada.' END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_EliminarMarca
    @IdMarca int, @Resultado bit OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE dbo.MARCA WHERE IdMarca = @IdMarca;
        SET @Resultado = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
        SET @Mensaje = CASE WHEN @Resultado = 1 THEN 'Marca eliminada.' ELSE 'Marca no encontrada.' END;
    END TRY
    BEGIN CATCH
        SET @Resultado = 0; SET @Mensaje = LEFT(ERROR_MESSAGE(), 500);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RegistrarProducto
    @Nombre varchar(500), @Descripcion varchar(500), @IdMarca int, @IdCategoria int,
    @Precio decimal(18,2), @Stock int, @Activo bit,
    @Resultado int OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT dbo.PRODUCTO (Nombre, Descripcion, IdMarca, IdCategoria, Precio, Stock, RutaImagen, NombreImagen, Activo, FechaRegistro)
        VALUES (@Nombre, @Descripcion, @IdMarca, @IdCategoria, @Precio, @Stock, '', '', @Activo, GETDATE());
        SET @Resultado = CONVERT(int, SCOPE_IDENTITY());
        SET @Mensaje = 'Producto creado.';
    END TRY
    BEGIN CATCH
        SET @Resultado = 0; SET @Mensaje = LEFT(ERROR_MESSAGE(), 500);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_EditarProducto
    @IdProducto int, @Nombre varchar(500), @Descripcion varchar(500), @IdMarca int, @IdCategoria int,
    @Precio decimal(18,2), @Stock int, @Activo bit,
    @Resultado bit OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.PRODUCTO
    SET Nombre = @Nombre, Descripcion = @Descripcion, IdMarca = @IdMarca, IdCategoria = @IdCategoria,
        Precio = @Precio, Stock = @Stock, Activo = @Activo
    WHERE IdProducto = @IdProducto;
    SET @Resultado = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
    SET @Mensaje = CASE WHEN @Resultado = 1 THEN 'Producto actualizado.' ELSE 'Producto no encontrado.' END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_RegistrarUsuario
    @Nombres varchar(100), @Apellidos varchar(100), @Correo varchar(100), @Clave varchar(150), @Activo bit,
    @Resultado int OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET @Resultado = 0;
    IF EXISTS (SELECT 1 FROM dbo.USUARIO WHERE Correo = @Correo)
    BEGIN SET @Mensaje = 'Ya existe un usuario con ese correo.'; RETURN; END;
    BEGIN TRY
        INSERT dbo.USUARIO (Nombres, Apellidos, Correo, Clave, Reestablecer, Activo, FechaRegistro)
        VALUES (@Nombres, @Apellidos, @Correo, @Clave, 1, @Activo, GETDATE());
        SET @Resultado = CONVERT(int, SCOPE_IDENTITY());
        SET @Mensaje = 'Usuario creado.';
    END TRY
    BEGIN CATCH
        SET @Resultado = 0; SET @Mensaje = LEFT(ERROR_MESSAGE(), 500);
    END CATCH
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_EditarUsuario
    @IdUsuario int, @Nombres varchar(100), @Apellidos varchar(100), @Correo varchar(100), @Activo bit,
    @Resultado bit OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.USUARIO SET Nombres = @Nombres, Apellidos = @Apellidos, Correo = @Correo, Activo = @Activo
    WHERE IdUsuario = @IdUsuario;
    SET @Resultado = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
    SET @Mensaje = CASE WHEN @Resultado = 1 THEN 'Usuario actualizado.' ELSE 'Usuario no encontrado.' END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_EliminarUsuario
    @IdUsuario int, @Resultado bit OUTPUT, @Mensaje varchar(500) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE dbo.USUARIO WHERE IdUsuario = @IdUsuario;
    SET @Resultado = CASE WHEN @@ROWCOUNT > 0 THEN 1 ELSE 0 END;
    SET @Mensaje = CASE WHEN @Resultado = 1 THEN 'Usuario eliminado.' ELSE 'Usuario no encontrado.' END;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ReporteDashboard
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        (SELECT COUNT(*) FROM dbo.CLIENTE) AS TotalCliente,
        (SELECT COUNT(*) FROM dbo.VENTA) AS TotalVenta,
        (SELECT COUNT(*) FROM dbo.PRODUCTO) AS TotalProducto;
END;
GO

CREATE OR ALTER PROCEDURE dbo.sp_ReporteVentas
    @fechainicio varchar(30), @fechafin varchar(30), @idtransaccion varchar(100)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @desde date = COALESCE(TRY_CONVERT(date, @fechainicio, 23), TRY_CONVERT(date, @fechainicio, 103), TRY_CONVERT(date, @fechainicio));
    DECLARE @hasta date = COALESCE(TRY_CONVERT(date, @fechafin, 23), TRY_CONVERT(date, @fechafin, 103), TRY_CONVERT(date, @fechafin));
    SELECT CONVERT(varchar(10), v.FechaVenta, 23) AS FechaVenta,
           LTRIM(RTRIM(ISNULL(c.Nombres, '') + ' ' + ISNULL(c.Apellidos, ''))) AS Cliente,
           p.Nombre AS Producto, p.Precio, dv.Cantidad, dv.Total, v.IdTransaccion
    FROM dbo.VENTA AS v
    INNER JOIN dbo.CLIENTE AS c ON c.IdCliente = v.IdCliente
    INNER JOIN dbo.DETALLE_VENTA AS dv ON dv.IdVenta = v.IdVenta
    INNER JOIN dbo.PRODUCTO AS p ON p.IdProducto = dv.IdProducto
    WHERE (@desde IS NULL OR v.FechaVenta >= @desde)
      AND (@hasta IS NULL OR v.FechaVenta < DATEADD(day, 1, @hasta))
      AND (NULLIF(LTRIM(RTRIM(@idtransaccion)), '') IS NULL OR v.IdTransaccion LIKE '%' + @idtransaccion + '%')
    ORDER BY v.FechaVenta DESC, v.IdVenta DESC;
END;
GO
