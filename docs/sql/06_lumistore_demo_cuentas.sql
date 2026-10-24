-- Organiza sección
SET NOCOUNT ON;
SET XACT_ABORT ON;

IF DB_NAME() <> N'lumistore'
    THROW 51000, 'Base inesperada: este script solo admite lumistore.', 1;

IF OBJECT_ID(N'dbo.USUARIO', N'U') IS NULL OR OBJECT_ID(N'dbo.CLIENTE', N'U') IS NULL
    THROW 51001, 'Faltan tablas USUARIO o CLIENTE; ejecute primero el esquema.', 1;

IF OBJECT_ID(N'dbo.VENTA', N'U') IS NOT NULL AND EXISTS (SELECT 1 FROM dbo.VENTA)
    THROW 51002, 'Hay ventas; no se modificaron cuentas. Use una base de demostración vacía.', 1;

IF EXISTS (SELECT 1 FROM dbo.USUARIO) OR EXISTS (SELECT 1 FROM dbo.CLIENTE)
    THROW 51003, 'Hay cuentas existentes; no se modificaron datos.', 1;

BEGIN TRANSACTION;

INSERT dbo.USUARIO (Nombres, Apellidos, Correo, Clave, Reestablecer, Activo, FechaRegistro)
VALUES
    ('Admin Lumi', 'Principal', 'admin1@lumistore.demo', '460bf070076b0e8a3b91a643e94abd6c1f46c9095cef05d164c97d6565bc8220', 1, 1, GETDATE()),
    ('Admin Lumi', 'Respaldo', 'admin2@lumistore.demo', '9c8540962a61d13cafa1d3e3258dd51d7f3a5b778d76dc491063a0c321707307', 1, 1, GETDATE());

INSERT dbo.CLIENTE (Nombres, Apellidos, Correo, Clave, Reestablecer, FechaRegistro)
VALUES
    ('Cliente', 'Demo Uno', 'cliente1@lumistore.demo', '2ca12a7b226db2c1be0ab2f7720b47b94facff9716299d60ad83f12776a1b30e', 1, GETDATE()),
    ('Cliente', 'Demo Dos', 'cliente2@lumistore.demo', '8190f0fd2dbfc5dab591774081f8c425522c0ee368e6e2b8a4729c1acb2832f6', 1, GETDATE());

COMMIT TRANSACTION;

SELECT N'USUARIO' AS Tabla, COUNT(*) AS Total FROM dbo.USUARIO
UNION ALL
SELECT N'CLIENTE', COUNT(*) FROM dbo.CLIENTE;
