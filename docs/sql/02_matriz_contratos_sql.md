# Matriz de contratos SQL de DBCARRITO

Estado: inferido del código fuente. No se pudo confirmar el esquema desplegado ni ejecutar consultas en SQL Server.

| Llamador C# | Objeto / consulta | Entradas observadas | Salidas / columnas esperadas | Observaciones |
|---|---|---|---|---|
| `CD_Carrito.ListarProducto` | `fn_obtenerCarritoCliente(@idcliente)` | `@idcliente` (int inferido) | `IdProducto`, `Nombre`, `Precio`, `RutaImagen`, `NombreImagen`, `DesMarca`, `Cantidad` | Lee función tabular. Cualquier error se transforma en lista vacía. |
| `CD_Carrito.ExisteCarrito` | `sp_ExisteCarrito` | `IdCliente`, `IdProducto` (AddWithValue) | `Resultado` (bit OUTPUT) | Tipos y longitudes no comprobados. |
| `CD_Carrito.OperacionCarrito` | `sp_OperacionCarrito` | `IdCliente`, `IdProducto`, `Sumar` | `Resultado` (bit OUTPUT), `Mensaje` (varchar(500) OUTPUT) | No comprobados concurrencia, existencias ni bloqueo de stock en servidor. |
| `CD_Carrito.CantidadEnCarrito` | `CARRITO` | `@idcliente` | suma de `cantidad` | Consulta parametrizada; nombre de tabla/columnas inferidos del SQL de aplicación. |
| `CD_Carrito.EliminarCarrito` | `sp_EliminarCarrito` | `IdCliente`, `IdProducto` | `Resultado` (bit OUTPUT) | Sin contrato verificado. |
| `TiendaController` ubicación | `DEPARTAMENTO`, `MUNICIPIO`, `CIUDAD` | `IdDepartamento`, `IdMunicipio` | IDs y `Descripcion`; CIUDAD usa además `IdDepartamento` | SQL inline y tipos de ID tratados como texto; usar tipos SQL explícitos tras comprobar esquema. |
| `CD_Venta.Registrar` | `usp_RegistrarVenta` | Cliente, totales, contacto, ciudad, teléfono, dirección, transacción y TVP `@DetalleVenta` | `@Resultado` bit OUTPUT, `@Mensaje` varchar(500) OUTPUT | El DataTable es Structured pero no asigna `TypeName`; contrato TVP no está definido en C#. No invocar hasta confirmar tipo. |
| `CD_Venta.ListarVentasPorCliente` | `VENTA` | `@IdCliente` | columnas de venta, `FechaVenta` | Consulta parametrizada; nulos/campos deben cotejarse. |
| `CD_Venta.ObtenerDetalleVenta` | `DETALLE_VENTA` + `PRODUCTO` | `@IdVenta` | detalle y producto | JOIN, columnas y relaciones inferidos. |
| `CD_*` restantes | procedimientos y consultas varias | Ver código `Datos/CD_*.cs` | Ver mapeos del lector | Completar tras diagnóstico de solo lectura y revisión línea por línea. |

`CD_Venta` está bajo namespace `CapaDatos` y `CN_Venta` bajo `CapaNegocio`, mientras los demás archivos usan `Datos`/`Negocio`. No se debe unificar sin revisar referencias y consumidores.

## Confirmación pendiente

La existencia de objetos, parámetros reales, TVP, índices, constraints, columnas y tipos no está comprobada. Ejecutar `00_diagnostico_lectura_dbcarrito.sql`, revisar diferencias contra esta matriz y documentar resultados antes de diseñar DDL. No se incluyen operaciones de escritura en los diagnósticos.
