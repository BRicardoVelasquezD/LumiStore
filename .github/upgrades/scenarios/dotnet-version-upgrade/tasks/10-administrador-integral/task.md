# 10-administrador-integral: Migrar y corregir Administración

Migrar las vistas, formularios y operaciones administrativas de forma incremental. Frontend revisará idioma español, diseño adaptable, accesibilidad y estados; Backend verificará reglas, controles de acceso por rol, validación de servidor y persistencia; QA probará casos permitidos y denegados con identidades autorizadas y datos controlados. No retirar `[Authorize]`, inventar permisos ni ejecutar cambios de datos destructivos. El esquema de base es compartido: `#skill:managing-shared-database-schema`.

**Hecho cuando**: las capacidades aprobadas respetan roles/permisos, el acceso no autorizado queda bloqueado, las vistas son responsive y españolas, y QA verifica que los cambios persistidos corresponden solo a acciones previstas.
