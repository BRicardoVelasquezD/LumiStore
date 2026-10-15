# 07-andamiaje-tienda: Crear el host nuevo para Tienda

Crear la aplicación ASP.NET Core paralela de Tienda con proxy incremental, configuración aislada y respuesta inicial comprobable, sin retirar ni interrumpir el host MVC 5. Frontend acordará idioma español, estructura y recursos compartibles; Backend establecerá límites de configuración y acceso a datos con el esquema compartido protegido; QA comprobará el arranque del host nuevo y del antiguo. La conexión a base y el esquema son compartidos: `#skill:managing-shared-database-schema`.

**Hecho cuando**: ambos hosts siguen disponibles, el nuevo responde a la ruta inicial, la configuración no contiene secretos y no se ha modificado el esquema.
