# 04-datos-modernos: Habilitar y validar Datos

Habilitar el destino moderno de Datos después de validar Entidad. Investigar las incompatibilidades de ADO.NET señaladas por la evaluación, revisar consultas y parámetros por uso real y conservar el comportamiento de lectura/escritura autorizado. No modificar el esquema ni la cadena LocalDB `DBCARRITO` sin evidencia y aprobación; `#skill:managing-shared-database-schema`. Backend verificará operaciones de datos y QA probará las rutas de acceso con datos controlados antes de continuar.

**Hecho cuando**: Datos compila para los destinos aprobados, conserva consumidores Framework, las pruebas de acceso a base aprobadas pasan y no se alteró el esquema compartido.
