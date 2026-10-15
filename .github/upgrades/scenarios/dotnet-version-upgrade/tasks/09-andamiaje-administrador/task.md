# 09-andamiaje-administrador: Crear el host nuevo de Administración

Crear la aplicación ASP.NET Core paralela para Administrador, manteniendo el host MVC 5 existente. Backend revisará autenticación y autorización y preparará el límite de autenticación remota sin inventar usuarios, tokens ni secretos; Frontend establecerá componentes administrativos en español y responsive; QA comprobará el arranque y rechazo de acceso no autorizado sin operaciones CRUD. El esquema de base es compartido: `#skill:managing-shared-database-schema`.

**Hecho cuando**: el host nuevo arranca, la ruta pública responde, las rutas protegidas requieren autorización, no se exponen secretos y el esquema sigue intacto.
