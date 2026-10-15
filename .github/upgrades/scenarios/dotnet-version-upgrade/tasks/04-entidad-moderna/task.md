# 04-entidad-moderna: Preparar Entidad para el destino moderno

Habilitar y validar el destino moderno para Entidad, que no tiene referencias internas y es la dependencia de Datos, Negocio y ambas aplicaciones. Mantener el contrato compatible con los consumidores existentes. Backend verificará los modelos y sus serializaciones; QA compilará los destinos aprobados y los consumidores Framework antes de avanzar.

**Hecho cuando**: Entidad compila en sus destinos aprobados sin errores ni advertencias y no rompe las referencias de consumidores existentes.

## Investigación confirmada

- Estado actual en el archivo del proyecto: SDK-style, `TargetFramework` singular `net472`. La tarea debe añadir multi-targeting en orden moderno-primero (`net10.0;net472`) para conservar a los consumidores MVC/.NET Framework mientras los nuevos hosts se validan.
- Alcance real: 18 archivos de proyecto (13 modelos de dominio, 4 modelos vacíos `Entidad/Paypal/*`, `Properties/AssemblyInfo.cs`, y el `.csproj`). El assembly info es explícito y `GenerateAssemblyInfo=false`; debe conservarse para evitar atributos duplicados.
- No se detectaron paquetes NuGet, referencias a otros proyectos, APIs incompatibles en assessment, ni `Directory.Build.props`/`global.json` que definan TFM. Dependencias reportadas son sólo referencias de ensamblado del framework. Los modelos usan tipos POCO y el análisis de compatibilidad registra 0 incidencias API para Entidad.
- El SDK requerido para `net10.0` está disponible; la validación de global.json informa que no hay archivo que limite la selección de SDK. El assessment de compatibilidad es anterior al estado actual de los proyectos y muestra formato clásico para Entidad, aunque el archivo actual es SDK-style.
- Orden de dependencias confirmado: Entidad precede a Datos, Negocio y ambos hosts. El build de solución anterior a esta tarea pasó. No hay proyecto de pruebas automatizadas detectado.
- Para cumplir la estrategia seleccionada, conservar `net472` además de habilitar `net10.0`; no habilitar pagos ni modificar los modelos PayPal placeholders.

## Bloqueo de edición

- En la tarea anterior quedó documentado que las ediciones del IDE a `Entidad.csproj` fallaron tres veces mientras la solución estaba abierta, con instrucción explícita de no repetir esos intentos. La herramienta de conversión confirma que Entidad ya es SDK-style, pero no cambia TFM. No hay herramienta dedicada disponible para cambiar el TFM; `unload_project` no está autorizado por las instrucciones actuales. Se detiene antes de editar para evitar repetir el bloqueo. Se requiere que el usuario cierre la solución o aplique un mecanismo de edición soportado y la recargue antes de reanudar.
