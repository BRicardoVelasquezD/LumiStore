# PayPal Sandbox

El carrito conserva los precios en COP. El servidor consulta una tasa USD/COP gratuita para crear el pedido PayPal en USD y vuelve a verificar el importe capturado antes de registrar la venta.

En la configuración de la aplicación **Tienda** de Azure, agrega estas variables de entorno:

| Nombre | Valor |
| --- | --- |
| `LUMISTORE_PAYPAL_CLIENT_ID` | Client ID de la aplicación REST sandbox |
| `LUMISTORE_PAYPAL_CLIENT_SECRET` | Secret vigente de la aplicación REST sandbox |
| `LUMISTORE_COP_PER_USD` | Tasa de respaldo manual opcional, en COP por USD |

El fallback de tasa usa punto decimal y no lleva separadores de miles. La tasa pública se consulta primero; solo se usa el fallback si el servicio no responde.

Guarda los valores secretos en Application Settings de Azure, nunca en el repositorio ni en el chat. Reinicia la aplicación **Tienda** después de guardarlos. Las variables SMTP existentes `LUMISTORE_SMTP_USERNAME` y `LUMISTORE_SMTP_PASSWORD` conservan sus nombres.

La integración apunta únicamente al endpoint sandbox y no realiza cobros reales. Para probar, usa una aplicación REST sandbox y una cuenta compradora sandbox.
