# 02-bibliotecas-sdk: Convertir las bibliotecas clásicas a formato SDK

Convertir Entidad, Datos y Negocio a formato SDK manteniendo inicialmente .NET Framework 4.7.2 y los contratos actuales. Frontend no aplica en esta tarea; Backend revisará referencias, recursos y dependencias NuGet, y QA comprobará compilación de las tres bibliotecas y consumidores actuales. No incluir las aplicaciones web MVC 5 en esta conversión.

**Hecho cuando**: las tres bibliotecas compilables conservan sus contratos y destino Framework; Tienda y Administrador siguen compilando y ninguna referencia necesaria se pierde.
