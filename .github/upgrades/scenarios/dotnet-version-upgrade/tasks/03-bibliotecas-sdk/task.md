# 03-bibliotecas-sdk: Convertir las bibliotecas clásicas a formato SDK

Convertir Entidad, Datos y Negocio a formato SDK manteniendo inicialmente .NET Framework 4.7.2 y los contratos actuales. Frontend no aplica en esta tarea; Backend revisará referencias, recursos y dependencias NuGet, y QA comprobará compilación de las tres bibliotecas y consumidores actuales. No incluir las aplicaciones web MVC 5 en esta conversión.

**Hecho cuando**: las tres bibliotecas compilables conservan sus contratos y destino Framework; Tienda y Administrador siguen compilando y ninguna referencia necesaria se pierde.

## Inventario de alcance e investigación (antes de cambios)

- **Orden topológico confirmado** por la herramienta de dependencias de la solución: `Entidad/Entidad.csproj` (sin referencias de proyecto) → `Datos/Datos.csproj` (referencia `Entidad`) → `Negocio/Negocio.csproj` (referencias `Datos` y `Entidad`). Los consumidores web son `Tienda/Tienda.csproj` y `Administrador/Administrador.csproj`; se excluyen de esta conversión porque se mantienen en MVC 5/System.Web y el plan selecciona side-by-side.
- **Estado inicial confirmado en assessment**: las tres bibliotecas son proyectos clásicos no SDK con TFM `net472`. Entidad y Negocio tienen dos incidencias de evaluación cada una (conversión SDK y TFM); Datos tiene 998 (dos del proyecto y 996 avisos de API `System.Data.SqlClient` incompatibles con el destino moderno). En esta tarea únicamente se resuelve el formato; se conserva `net472` exactamente y las APIs/contratos no se refactorizan. Revisar las incompatibilidades de Datos en la tarea que habilite su destino moderno.
- **Archivos y referencias**: Entidad contiene modelos, incluidos modelos PayPal; Datos contiene las clases `CD_*` y `Conexion.cs`; Negocio contiene `CN_*`, incluida `CN_Recursos.cs`. Referencias de proyecto y ensamblados consultadas mediante inventario de dependencias. No hay `packages.config` en estas bibliotecas; la búsqueda de la solución solo lo encontró en los dos hosts web excluidos.
- **Paquetes/tests**: los metadatos consultados no muestran dependencias NuGet de estas tres bibliotecas. La detección de proyectos de prueba no identificó proyectos de test en la solución. No se encontraron marcadores `STUB:` en los fuentes consultados.
- **Descomposición**: no necesaria: son tres proyectos, dentro del umbral de conversión única del escenario; la conversión se hará en serie y en orden de dependencia, con validación tras cada proyecto para contener cualquier fallo.
- **Mecanismo seguro ante el bloqueo anterior**: utilizar exclusivamente `convert_project_to_sdk_style`, secuencialmente, no editar manualmente XML de `.csproj` ni repetir los intentos de edición del IDE que fallaron con la solución abierta.
- **Validación prevista**: tras cada conversión, compilar directamente el proyecto y la solución con MSBuild/IDE en Debug para comprobar consumidores. Resolver avisos en los proyectos tocados; comprobar TFM `net472`, configuración de proyecto/referencias y ausencia de `packages.config` asociada. No ejecutar acciones contra la base de datos.

## Confirmación de investigación antes de conversión

- Se confirmó con el orden topológico de la solución: `Entidad` → `Datos` → `Negocio` → hosts web. Los tres proyectos de biblioteca siguen siendo clásicos, `net472`, sin paquetes NuGet en assessment; convertir en ese orden con la herramienta específica, un proyecto cada vez, sin cambiar TFM.
- El assessment por proyecto confirma: Entidad 2 incidencias obligatorias de proyecto (SDK y TFM); Datos 2 obligatorias más 996 avisos potenciales de API; Negocio 2 obligatorias. Los avisos de API de Datos son para la futura habilitación del TFM moderno, fuera de esta conversión estructural.
- Build base de `LumiStore.slnx` en configuración predeterminada: correcto antes de cambios; no se identificaron proyectos de pruebas en el inventario previo.
- No se hará edición manual de `.csproj`; se usará `convert_project_to_sdk_style` secuencial y se validará inmediatamente después de cada biblioteca. La base de datos no forma parte de la conversión.

## Descubrimiento SQL independiente (solo lectura)

- Se comprobó el catálogo de `DBCARRITO` mediante SELECT de metadatos; no se modificaron datos ni objetos. Hay 11 tablas, 8 PK, 0 FK, 0 procedimientos, 0 vistas y 0 triggers; existe `dbo.fn_obtenerCarritoCliente(int)`.
- El código de `Datos/CD_Carrito.cs`, `CD_Categoria.cs`, `CD_Cliente.cs`, `CD_Marca.cs`, `CD_Producto.cs`, `CD_Reporte.cs`, `CD_Usuario.cs` y `CD_Venta.cs` invoca 18 nombres de procedimientos que no existen según el catálogo: `sp_ExisteCarrito`, `sp_OperacionCarrito`, `sp_EliminarCarrito`, `sp_RegistrarCategoria`, `sp_EditarCategoria`, `sp_EliminarCategoria`, `sp_RegistrarCliente`, `sp_RegistrarMarca`, `sp_EditarMarca`, `sp_EliminarMarca`, `sp_RegistrarProducto`, `sp_EditarProducto`, `sp_ReporteDashboard`, `sp_ReporteVentas`, `sp_RegistrarUsuario`, `sp_EditarUsuario`, `sp_EliminarUsuario`, `usp_RegistrarVenta`. Esas rutas no pueden funcionar contra el catálogo actual.
- Hay una discrepancia reproducible por nombres: `CD_Venta.ListarVentasPorCliente` solicita `VENTA.FechaRegistro`, mientras la tabla real expone `FechaVenta`; el método captura la excepción y devuelve una lista vacía. La función sí expone las columnas esperadas por el mapeo de carrito.
- Este hallazgo corresponde a corrección funcional/SQL posterior, no a conversión SDK. No ejecutar DDL ni CRUD de escritura sobre `DBCARRITO`; el usuario debe decidir autorizar cualquier cambio de esquema y proveer una BD de prueba aislada para CRUD con escrituras.

## Resultado y verificación

- `convert_project_to_sdk_style` informó que `Entidad/Entidad.csproj` y `Datos/Datos.csproj` ya estaban en formato SDK; no se modificaron. La herramienta convirtió `Negocio/Negocio.csproj` correctamente. No se editó manualmente ningún `.csproj`.
- Los tres proyectos mantienen `<TargetFramework>net472</TargetFramework>`. Se verifican las referencias esperadas: Datos → Entidad; Negocio → Datos y Entidad. Los contratos fuente no fueron refactorizados en esta tarea.
- Builds de Entidad, Datos y Negocio tras las comprobaciones, y build final de toda la solución: correctos. La detección de pruebas no encontró proyectos de test; no hay pruebas automatizadas disponibles para estas bibliotecas.
- La evaluación de compatibilidad anterior señalaba los tres proyectos como clásicos; la inspección y herramienta actuales mostraron que Entidad y Datos ya estaban en SDK, por lo que ese dato del assessment no refleja el estado presente. Negocio sí fue convertido en esta ejecución.
