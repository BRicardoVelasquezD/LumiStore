# Progreso — 03-bibliotecas-sdk

## Cambios realizados
- `Entidad/Entidad.csproj` convertido a formato SDK mediante `convert_project_to_sdk_style`.
- `Datos/Datos.csproj` convertido a formato SDK mediante la misma herramienta; se conserva la referencia de proyecto a `Entidad`.
- `Negocio/Negocio.csproj` ya era SDK-style pese a que el assessment anterior lo marcaba como clásico. La herramienta dedicada confirmó el estado existente; no se editó.
- Los tres proyectos conservan `net472`, `OutputType=Library` y `GenerateAssemblyInfo=false`; no se cambiaron contratos ni código fuente. Se conservaron las referencias: Datos → Entidad; Negocio → Datos y Entidad.
- No hay `packages.config` en estas bibliotecas; los archivos localizados pertenecen a los dos hosts MVC excluidos.
- Se enriqueció `task.md` con investigación, dependencias, evaluación y validación prevista.

## Validación
- Compilación directa de Entidad, Datos y Negocio: correcta.
- Compilación de `LumiStore.slnx` después de cada conversión/validación: correcta en las tres comprobaciones, incluidos los consumidores Tienda y Administrador.
- La última salida de Visual Studio indicó 0 errores y 5 proyectos actualizados. No se reportaron advertencias en los resultados disponibles.
- La detección no encontró proyectos de prueba; no había suite automatizada que ejecutar.
- No se ejecutó ninguna operación de base de datos durante esta tarea.

## Incidencias y decisiones
- Se evitó repetir los intentos fallidos de edición manual de `Entidad.csproj`. La conversión se hizo con la herramienta soportada, secuencialmente y siguiendo el orden topológico.
- El assessment está desfasado: Negocio ya era SDK-style en el estado observado. No se modificó su archivo.
- Las 996 incidencias de API `System.Data.SqlClient` en Datos están asociadas al destino moderno propuesto; esta tarea solo convierte formato y conserva `net472`, por lo que se difieren a la tarea que habilita ese TFM.
- Las notas SQL previas de este archivo no son evidencia reproducible adjunta. Se verificarán de nuevo mediante un inventario de catálogo estrictamente de solo lectura y se entregará un artefacto SQL separado; no se hicieron cambios de esquema ni pruebas CRUD con escritura.
