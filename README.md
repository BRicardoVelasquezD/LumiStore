# LumiStore

Plataforma de comercio electrónico construida con ASP.NET MVC 5 para una tienda en línea y un panel privado de administración. El repositorio incluye la aplicación de tienda, el sitio administrativo y las capas compartidas de entidades, acceso a datos y lógica de negocio.

> **Estado del sistema:** tienda y administrador están publicados en Azure App Service y usan Azure SQL Database `lumistore`. Las rutas de catálogo responden correctamente. El correo SMTP y las credenciales privadas de PayPal aún requieren configuración; hasta entonces, los flujos de correo y pago real no están habilitados. Las dos cuentas nuevas requieren cambiar su contraseña al primer acceso.

## Índice

- [Aplicaciones](#aplicaciones)
- [Arquitectura](#arquitectura)
- [Requisitos](#requisitos)
- [Compilar en Windows](#compilar-en-windows)
- [Base de datos](#base-de-datos)
- [Configuración local y Azure](#configuración-local-y-azure)
- [Rutas principales](#rutas-principales)
- [Pruebas operativas](#pruebas-operativas)
- [Publicación](#publicación)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Seguridad y operación](#seguridad-y-operación)
- [Resolución de problemas](#resolución-de-problemas)

## Aplicaciones

| Aplicación | URL | Inicio |
|---|---|---|
| Tienda | [tienda20261015005938-dzg8cuhwgzg0b7gp.westus3-01.azurewebsites.net](https://tienda20261015005938-dzg8cuhwgzg0b7gp.westus3-01.azurewebsites.net) | `/` o `/Tienda/Index` |
| Administrador | [administrador20261015004448-h2d9bcayd7e3eqc2.westus3-01.azurewebsites.net](https://administrador20261015004448-h2d9bcayd7e3eqc2.westus3-01.azurewebsites.net) | `/Acceso/Index` |

Ambas aplicaciones se ejecutan como sitios ASP.NET MVC 5 sobre .NET Framework 4.7.2 en Windows App Service. La base configurada es Azure SQL: servidor `lumistore.database.windows.net`, base `lumistore`, puerto 1433. La conexión usa identidad administrada de la aplicación; no se almacena una contraseña SQL en el repositorio.

### Guía rápida

- **Comprar:** entra a la tienda, explora el catálogo, agrega productos al carrito y crea una cuenta de cliente para iniciar sesión.
- **Administrar:** entra al sitio administrativo con una cuenta de personal autorizado. La cuenta inicial debe cambiar la contraseña antes de llegar al panel.
- **Para desarrolladores:** abre la solución en Visual Studio, restaura NuGet, configura acceso a Azure SQL y sigue las instrucciones de compilación y base de datos más abajo.

El inicio de sesión funciona para las cuentas creadas y, por seguridad, ambas muestran primero el formulario para cambiar la contraseña temporal. La tienda permite consultar el catálogo; el correo de bienvenida/recuperación y el pago real requieren las credenciales externas descritas en configuración.

## Arquitectura

```text
Tienda ──────────┐
                 ├── Negocio ── Datos ── Azure SQL Database
Administrador ───┘      │
                       Entidad
```

- **Tienda**: catálogo, carrito, acceso y registro de clientes, órdenes y flujo de pago PayPal.
- **Administrador**: autenticación administrativa, panel, mantenimiento de productos, categorías y marcas, ubicaciones, usuarios y reportes.
- **Entidad**: modelos compartidos (`Producto`, `Cliente`, `Usuario`, `Venta`, `Carrito`, etc.).
- **Datos**: consultas SQL y llamadas a procedimientos almacenados. Las conexiones se crean mediante identidad administrada.
- **Negocio**: validaciones y coordinación entre controladores y acceso a datos.

El enrutamiento MVC predeterminado lleva la tienda a `TiendaController.Index` y el panel administrativo a `HomeController.Index`. Las vistas están bajo `Tienda/Views` y `Administrador/Views`.

## Requisitos

- Windows con Visual Studio 2022 (carga de trabajo **ASP.NET y desarrollo web**) o MSBuild para .NET Framework.
- .NET Framework 4.7.2 Developer Pack/Targeting Pack.
- NuGet para restaurar los paquetes declarados por `packages.config`.
- Para acceso local a Azure SQL: Azure CLI (`az`), sesión autenticada y permisos de red y base de datos. Para desarrollo se recomienda usar una identidad administrada en Azure o una identidad de desarrollador autenticada; no guardar credenciales en archivos versionados.

## Compilar en Windows

Abre `LumiStore.slnx` en Visual Studio y restaura los paquetes NuGet. También se puede compilar desde una consola de desarrollador de Visual Studio:

```powershell
nuget restore .\LumiStore.slnx
msbuild .\LumiStore.slnx /p:Configuration=Release /m
```

Si la versión de NuGet/MSBuild no reconoce el formato `.slnx`, abre la solución desde Visual Studio 2022 o compila los proyectos `Tienda/Tienda.csproj` y `Administrador/Administrador.csproj` con MSBuild. Los proyectos comparten las bibliotecas `Entidad`, `Datos` y `Negocio`. No borres `packages/` si estás trabajando sin conexión; en línea se puede regenerar restaurando NuGet.

## Base de datos

Los scripts de instalación están en `docs/sql/`. En una base vacía, conéctate a la base objetivo y ejecútalos en este orden:

1. `docs/sql/07_provision_lumistore_schema.sql` — crea el esquema de LumiStore. **Se detiene si encuentra tablas preexistentes**, para evitar sobrescribir datos.
2. `docs/sql/08_provision_lumistore_procedures.sql` — instala o actualiza los procedimientos almacenados requeridos por las capas de datos.
3. `docs/sql/09_seed_catalog_reference.sql` — agrega el catálogo de referencia inicial. Solo si las tablas objetivo están vacías.

Ejemplo con `sqlcmd` (autenticación Entra ID interactiva):

```powershell
sqlcmd -S "tcp:lumistore.database.windows.net,1433" -d lumistore -G -b -i .\docs\sql\07_provision_lumistore_schema.sql
sqlcmd -S "tcp:lumistore.database.windows.net,1433" -d lumistore -G -b -i .\docs\sql\08_provision_lumistore_procedures.sql
sqlcmd -S "tcp:lumistore.database.windows.net,1433" -d lumistore -G -b -i .\docs\sql\09_seed_catalog_reference.sql
```

Revisa los otros scripts de `docs/sql/` antes de ejecutarlos: varios son diagnósticos o migraciones específicas del esquema y no forman parte de la instalación inicial. Haz una copia de seguridad antes de cualquier migración sobre datos existentes.

## Configuración local y Azure

### Base de datos e identidad

Los `Web.config` contienen el servidor y nombre de base, pero no secreto SQL. En Azure, habilita una identidad administrada para cada App Service y dale a esa identidad permisos mínimos en `lumistore` para leer/escribir los datos que la aplicación necesita y ejecutar procedimientos. En local, inicia sesión con Azure CLI (`az login`) usando una identidad que tenga los mismos permisos y una regla de firewall limitada a la IP de desarrollo. La conexión de `Datos/Conexion.cs` adquiere un token de Azure SQL; si el token no se obtiene, revisa la identidad y la configuración de Entra ID.

No agregues reglas de firewall abiertas a todo Internet ni guardes cadenas con contraseñas en `Web.config`, capturas, logs o commits.

La base está en el nivel serverless y se pausa automáticamente tras un periodo de inactividad para limitar consumo. La primera consulta después de una pausa puede demorar mientras Azure reactiva la base; los sitios permiten más tiempo de conexión para tolerar ese arranque. El auto-pausado se mantiene activo.

### Variables de aplicación

Configura secretos desde **App Service → Configuración → Variables de entorno** o desde un almacén de secretos; no los incluyas en este repositorio.

| Variable | Uso | Requerida para |
|---|---|---|
| `LUMISTORE_SMTP_USERNAME` | Cuenta autenticada de Gmail/SMTP | Envío de bienvenida y restablecimiento de contraseña |
| `LUMISTORE_SMTP_PASSWORD` | Contraseña de aplicación SMTP | Envío de correo |
| `LUMISTORE_PAYPAL_CLIENT_ID` | Identificador de aplicación PayPal | Inicializar checkout |
| `LUMISTORE_PAYPAL_CLIENT_SECRET` | Secreto privado PayPal | Obtener token y confirmar pago |
| `LUMISTORE_COP_PER_USD` | Tasa COP por USD | Opcional; tipo de cambio de la tienda |

El SMTP configurado por el código es `smtp.gmail.com:587` con TLS. Para Gmail, utiliza una contraseña de aplicación apropiada y verifica el remitente configurado. Sin variables SMTP, los flujos de registro/restablecimiento que envían correo fallan; no reportan éxito al cliente.

El código PayPal requiere credenciales válidas de Sandbox o Live según el entorno. Configura la misma modalidad para el ID y el secreto y prueba el ciclo completo de orden antes de habilitar pagos reales.

## Rutas principales

Las rutas MVC siguen el patrón `/{controller}/{action}/{id?}`.

| Sitio | Ruta | Propósito |
|---|---|---|
| Tienda | `/` | Página de inicio y catálogo |
| Tienda | `/Tienda/Index` | Inicio explícito |
| Tienda | `/Acceso/Index` | Acceso de cliente |
| Tienda | `/Acceso/Registrar` | Registro de cliente (revisar acción/vista disponibles) |
| Administrador | `/Acceso/Index` | Inicio de sesión administrativo |
| Administrador | `/Home/Index` | Panel principal tras autenticación |
| Administrador | `/Mantenimiento/...` | Operaciones de mantenimiento y catálogo |

Las acciones concretas y verbos HTTP pueden variar por controlador. Consulta las vistas y controladores antes de integrar clientes externos.

## Pruebas operativas

Comprobaciones realizadas durante el mantenimiento del entorno:

- Tienda y administrador desplegados y respondiendo con HTTP 200 en sus páginas principales.
- Catálogo de categorías devuelve 12 registros; consulta de productos por categoría responde con datos.
- Base de datos contiene el esquema y procedimientos necesarios y los catálogos de referencia fueron cargados.
- La identidad administrada de ambas aplicaciones tiene permisos de base de datos.
- Las cuentas de cliente y administrador aceptan el acceso y llevan a su pantalla de cambio obligatorio de contraseña.
- El endpoint de categorías devuelve 12 categorías y el filtro de una categoría devuelve productos y marcas.
- Envío SMTP y pago PayPal real no están validados porque no hay secretos configurados.

Al configurar correo y PayPal, valida el registro, recuperación de contraseña, carrito, creación de orden y flujo PayPal Sandbox. Después del cambio inicial de clave, prueba también las funciones CRUD y reportes con una cuenta de administrador autorizada.

## Publicación

Publica cada aplicación desde Visual Studio/MSBuild o desde un pipeline de despliegue aprobado. Antes de publicar:

1. Restaura NuGet y compila `Release`.
2. Configura identidad administrada, acceso SQL y variables de entorno en el App Service destino.
3. Confirma los nombres de la base y el slot/entorno para no apuntar a producción por error.
4. Despliega la salida publicada de `Tienda` y `Administrador` por separado.
5. Revisa los registros de App Service y efectúa las comprobaciones de [pruebas operativas](#pruebas-operativas).

No guardes paquetes de publicación, contraseñas, tokens o archivos de configuración con secretos dentro del repositorio.

## Estructura del repositorio

```text
LumiStore/
├── Administrador/       # Sitio MVC administrativo
├── Tienda/              # Sitio MVC de clientes
├── Datos/               # Acceso a Azure SQL y procedimientos
├── Negocio/             # Reglas de negocio
├── Entidad/             # Modelos compartidos
├── docs/sql/            # Instalación, diagnóstico y migraciones SQL
├── packages/            # Dependencias NuGet de proyectos clásicos
├── LumiStore.slnx       # Solución de Visual Studio
└── README.md
```

Las carpetas `.vs/`, `artifacts/`, `output/` y los resultados de compilación son caché o productos generados y no forman parte del producto desplegable. El archivo `tmp/imagegen/lumistore-products.jsonl` se conserva como insumo del catálogo visual.

## Seguridad y operación

- Usa una identidad diferente y de privilegios mínimos para cada App Service.
- Utiliza contraseñas largas, únicas y no compartidas por correo o en documentación pública.
- Revoca o cambia las claves temporales después del primer acceso.
- Habilita SMTP y PayPal únicamente mediante variables/almacén seguro y rota secretos periódicamente.
- Limita el firewall SQL a rangos de IP necesarios y elimina reglas temporales al terminar el mantenimiento.
- Programa respaldos y revisa logs de App Service/Azure SQL cuando haya fallos.
- La tabla `USUARIO` corresponde a personal administrativo; `CLIENTE` corresponde a compradores. Son registros separados aunque compartan dirección de correo.

## Resolución de problemas

| Síntoma | Revisión recomendada |
|---|---|
| Error de inicio de sesión o catálogo vacío | Verifica conectividad SQL, identidad administrada, permisos, firewall y que los procedimientos/tablas estén instalados. Los errores DAL se manejan de forma conservadora y pueden ocultar detalles al usuario. |
| Error al obtener token SQL | Comprueba la identidad administrada del App Service o la sesión Azure CLI local, y confirma que el principal exista en la base. |
| Error al enviar correo | Configura usuario y contraseña SMTP como variables de entorno y revisa el remitente, TLS y políticas de Gmail. |
| Checkout PayPal no inicia | Confirma las variables `LUMISTORE_PAYPAL_*`, la modalidad Sandbox/Live y los registros del sitio. |
| Falta una DLL en publicación | Restaura NuGet, compila `Release` y verifica que la dependencia esté marcada/copiada en la salida publicada. |
| Error HTTP 500 tras un cambio | Consulta logs del App Service y valida que los archivos publicados correspondan a la misma compilación que sus bibliotecas. |
