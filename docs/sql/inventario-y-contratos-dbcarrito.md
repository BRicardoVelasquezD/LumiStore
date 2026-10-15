# Inventario de catálogo y contratos de datos — LumiStore / DBCARRITO

## Alcance y método

- Inventario ejecutado con Windows Authentication contra `(localdb)\MSSQLLocalDB`, base `DBCARRITO`.
- El script [diagnostico_catalogo_dbcarrito.sql](diagnostico_catalogo_dbcarrito.sql) se ejecutó correctamente antes y después del DDL. El script solo consulta `sys.*`; no lee filas de negocio ni ejecuta CRUD.
- Se aplicó por separado [relaciones_seguidas_dbcarrito.sql](relaciones_seguidas_dbcarrito.sql), autorizado expresamente por el usuario. Añadió únicamente restricciones PK/FK dentro de una transacción; no contiene DML de filas. El preflight encontró 0 duplicados/nulos para las claves geográficas y 0 huérfanos para las relaciones incluidas; `WITH CHECK` validó de nuevo las relaciones durante el DDL.
- No se insertó, actualizó ni eliminó información de negocio. El inventario solo devuelve metadatos; el preflight devuelve únicamente recuentos agregados.

## Evidencia física verificada

- **Línea base antes del DDL**: 11 tablas, 8 claves primarias, 0 claves foráneas, 0 procedimientos, 1 función tabular inline, 0 triggers, 0 tipos de tabla, 0 defaults y 0 checks.
- **Estado posterior al DDL**: 11 tablas, 11 claves primarias, 10 claves foráneas habilitadas y confiables, 0 procedimientos, 1 función tabular inline, 0 triggers, 0 tipos de tabla de usuario, 0 defaults y 0 checks. La única rutina sigue siendo `dbo.fn_obtenerCarritoCliente(@idcliente int)`.

### Tablas y columnas observadas

| Tabla | Columnas observadas |
|---|---|
| `CARRITO` | `IdCarrito int IDENTITY` (PK), `IdCliente int`, `IdProducto int`, `Cantidad int` |
| `CATEGORIA` | `IdCategoria int IDENTITY` (PK), `Descripcion varchar(100)`, `Activo bit`, `FechaRegistro datetime` |
| `CIUDAD` | `IdCiudad varchar(6)`, `Descripcion varchar(45)`, `IdMunicipio varchar(4)`, `IdDepartamento varchar(2)` |
| `CLIENTE` | `IdCliente int IDENTITY` (PK), `Nombres varchar(100)`, `Apellidos varchar(100)`, `Correo varchar(100)`, `Clave varchar(150)`, `Reestablecer bit`, `FechaRegistro datetime` |
| `DEPARTAMENTO` | `IdDepartamento varchar(2)`, `Descripcion varchar(45)` |
| `DETALLE_VENTA` | `IdDetalleVenta int IDENTITY` (PK), `IdVenta int`, `IdProducto int`, `Cantidad int`, `Total decimal(18,2)` |
| `MARCA` | `IdMarca int IDENTITY` (PK), `Descripcion varchar(100)`, `Activo bit`, `FechaRegistro datetime` |
| `MUNICIPIO` | `IdMunicipio varchar(4)`, `Descripcion varchar(45)`, `IdDepartamento varchar(2)` |
| `PRODUCTO` | `IdProducto int IDENTITY` (PK), `Nombre varchar(500)`, `Descripcion varchar(500)`, `IdMarca int`, `IdCategoria int`, `Precio decimal(18,2)`, `Stock int`, `RutaImagen varchar(100)`, `NombreImagen varchar(100)`, `Activo bit`, `FechaRegistro datetime` |
| `USUARIO` | `IdUsuario int IDENTITY` (PK), `Nombres varchar(100)`, `Apellidos varchar(100)`, `Correo varchar(100)`, `Clave varchar(150)`, `Reestablecer bit`, `Activo bit`, `FechaRegistro datetime` |
| `VENTA` | `IdVenta int IDENTITY` (PK), `IdCliente int`, `TotalProducto int`, `MontoTotal decimal(18,2)`, `Contacto varchar(50)`, `IdCiudad varchar(10)`, `Telefono varchar(50)`, `Direccion varchar(500)`, `IdTransaccion varchar(50)`, `FechaVenta datetime` |

Las ocho PK iniciales correspondían a `CARRITO`, `CATEGORIA`, `CLIENTE`, `DETALLE_VENTA`, `MARCA`, `PRODUCTO`, `USUARIO` y `VENTA`. Tras el DDL se añadieron PK a `CIUDAD(IdCiudad)`, `DEPARTAMENTO(IdDepartamento)` y `MUNICIPIO(IdMunicipio)`. También se añadieron y verificaron estas diez FK, todas `NO ACTION` para eliminar/actualizar:

- `CARRITO(IdCliente)` → `CLIENTE(IdCliente)`
- `CARRITO(IdProducto)` → `PRODUCTO(IdProducto)`
- `PRODUCTO(IdMarca)` → `MARCA(IdMarca)`
- `PRODUCTO(IdCategoria)` → `CATEGORIA(IdCategoria)`
- `DETALLE_VENTA(IdVenta)` → `VENTA(IdVenta)`
- `DETALLE_VENTA(IdProducto)` → `PRODUCTO(IdProducto)`
- `VENTA(IdCliente)` → `CLIENTE(IdCliente)`
- `MUNICIPIO(IdDepartamento)` → `DEPARTAMENTO(IdDepartamento)`
- `CIUDAD(IdMunicipio)` → `MUNICIPIO(IdMunicipio)`
- `CIUDAD(IdDepartamento)` → `DEPARTAMENTO(IdDepartamento)`

No se añadió la relación `VENTA.IdCiudad` → `CIUDAD.IdCiudad`: los tipos observados difieren (`varchar(10)` frente a `varchar(6)`). No se alteraron longitudes ni valores.

## Contratos que espera el código y diferencias físicas

El catálogo no contiene ninguno de los siguientes **18 procedimientos** invocados por `Datos`. Se enumeran como expectativas del código, no como objetos existentes ni como autorización para crearlos:

| Objeto esperado | Archivo/método | Parámetros de entrada observados | Salidas/resultado esperado |
|---|---|---|---|
| `sp_ExisteCarrito` | `CD_Carrito.ExisteCarrito` | `IdCliente`, `IdProducto` | `Resultado bit OUTPUT` |
| `sp_OperacionCarrito` | `CD_Carrito.OperacionCarrito` | `IdCliente`, `IdProducto`, `Sumar` | `Resultado bit OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_EliminarCarrito` | `CD_Carrito.EliminarCarrito` | `IdCliente`, `IdProducto` | `Resultado bit OUTPUT` |
| `sp_RegistrarCategoria` | `CD_Categoria.Registrar` | `Descripcion`, `Activo` | `Resultado int OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_EditarCategoria` | `CD_Categoria.Editar` | `IdCategoria`, `Descripcion`, `Activo` | `Resultado bit OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_EliminarCategoria` | `CD_Categoria.Eliminar` | `IdCategoria` | `Resultado bit OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_RegistrarCliente` | `CD_Cliente.Registrar` | `Nombres`, `Apellidos`, `Correo`, `Clave` | `Resultado int OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_RegistrarMarca` | `CD_Marca.Registrar` | `Descripcion`, `Activo` | `Resultado int OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_EditarMarca` | `CD_Marca.Editar` | `IdMarca`, `Descripcion`, `Activo` | `Resultado bit OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_EliminarMarca` | `CD_Marca.Eliminar` | `IdMarca` | `Resultado bit OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_RegistrarProducto` | `CD_Producto.Registrar` | `Nombre`, `Descripcion`, `IdMarca`, `IdCategoria`, `Precio`, `Stock`, `Activo` | `Resultado int OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_EditarProducto` | `CD_Producto.Editar` | `IdProducto`, `Nombre`, `Descripcion`, `IdMarca`, `IdCategoria`, `Precio`, `Stock`, `Activo` | `Resultado bit OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_RegistrarUsuario` | `CD_Usuario.Registrar` | `Nombres`, `Apellidos`, `Correo`, `Clave`, `Activo` | `Resultado int OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_EditarUsuario` | `CD_Usuario.Editar` | `IdUsuario`, `Nombres`, `Apellidos`, `Correo`, `Activo` | `Resultado bit OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_EliminarUsuario` | `CD_Usuario.Eliminar` | `IdUsuario` | `Resultado bit OUTPUT`, `Mensaje varchar(500) OUTPUT` |
| `sp_ReporteDashboard` | `CD_Reporte.VerDashboard` | Ninguno | Primera fila con `TotalCliente`, `TotalVenta`, `TotalProducto` |
| `sp_ReporteVentas` | `CD_Reporte.Ventas` | `fechainicio`, `fechafin`, `idtransaccion` | Filas con `FechaVenta`, `Cliente`, `Producto`, `Precio`, `Cantidad`, `Total`, `IdTransaccion` |
| `usp_RegistrarVenta` | `CD_Venta.Registrar` | `IdCliente`, `TotalProducto`, `MontoTotal`, `Contacto`, `IdCiudad`, `Telefono`, `Direccion`, `IdTransaccion`, `DetalleVenta` | `Resultado bit OUTPUT`, `Mensaje varchar(500) OUTPUT`; `DetalleVenta` se marca `SqlDbType.Structured` |

`@DetalleVenta` exige un tipo de tabla SQL compatible. No existe ningún tipo de tabla de usuario en el catálogo y el parámetro C# no establece `TypeName`; las columnas y tipos del `DataTable` no quedan definidos por el código revisado. **No se deduce ni se inventa el tipo TVP.**

### Otras operaciones SQL encontradas en el código

- **Lecturas directas**: listados de categoría, cliente, marca, producto y usuario; listas de departamento/municipio/ciudad; conteo de carrito; consulta de ventas por cliente y detalle de venta. La función del carrito existe y su forma coincide con el mapeo.
- **Escrituras directas en el código (no ejecutadas)**: cambio/restablecimiento de clave en `CLIENTE` y `USUARIO`; actualizar rutas/nombres de imagen en `PRODUCTO`; borrar producto directamente. Son operaciones que también requieren una base de pruebas aislada antes de probar.
- **Venta por cliente**: `Datos/CD_Venta.cs::ListarVentasPorCliente` ahora selecciona `FechaVenta AS FechaRegistro`, acorde con el esquema físico y con el nombre leído por C#. La compilación de Datos y de la solución pasa. Esto corrige esa discrepancia de columna, pero no crea ventas.
- **Compra incompleta en la web**: `Tienda/Controllers/TiendaController.cs::ProcesarCompra` actualmente devuelve éxito y un identificador `TRX-...` generado localmente, pero el método contiene un comentario de que falta implementar el registro mediante `CN_Venta`; no persiste una venta ni confirma pago. No se seleccionó proveedor, no se activó pago real y no se llamó a esa ruta durante la auditoría.
- Varias funciones de acceso capturan excepciones y devuelven listas vacías/`false`, lo que puede ocultar la causa de objetos SQL ausentes en la interfaz.

## Estado de funcionalidad y límites

- La aplicación **no puede considerarse CRUD completa** contra el catálogo actual: faltan todos los SP enumerados; también falta el tipo TVP de venta. Los endpoints CRUD que los invocan fallarán o devolverán fallos según el manejo de excepciones.
- Esta entrega contiene un diagnóstico repetible y una matriz verificable de expectativas. El único DDL aplicado fue el de claves primarias/geográficas y diez relaciones listadas arriba. No se crearon procedimientos ni tipos TVP.
- Para completar las operaciones CRUD faltan los procedimientos y el tipo TVP. Se necesita acordar los detalles de negocio no expresados por el código y contar con una base aislada autorizada para validar escrituras. La integración de pagos reales queda fuera sin una decisión explícita de proveedor y credenciales de prueba.
- SMTP se deja sin cambios, de acuerdo con la confirmación del usuario de que su configuración existente es correcta.
