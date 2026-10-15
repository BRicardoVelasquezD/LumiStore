using Datos;
using Entidad;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Negocio
{
    public class CN_Cliente
    {
        private CD_Cliente objDato = new CD_Cliente();

        public List<Cliente> Listar() => objDato.Listar();

        public int Registrar(Cliente obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(obj.Nombres)) Mensaje = "El nombre es obligatorio";
            if (string.IsNullOrWhiteSpace(obj.Apellidos)) Mensaje = "El apellido es obligatorio";
            if (string.IsNullOrWhiteSpace(obj.Correo)) Mensaje = "El correo es obligatorio";
            if (string.IsNullOrWhiteSpace(obj.Clave)) Mensaje = "La contraseña es obligatoria";

            if (string.IsNullOrEmpty(Mensaje))
            {
                // Encriptamos la clave antes de guardar
                obj.Clave = CN_Recursos.ConvertirSha256(obj.Clave);
                return objDato.Registrar(obj, out Mensaje);
            }
            return 0;
        }

        public bool CambiarClave(int idcliente, string nuevaclave, out string Mensaje)
        {
            return objDato.CambiarClave(idcliente, nuevaclave, out Mensaje);
        }

        public bool ReestablecerClave(string correo, out string Mensaje)
        {
            Mensaje = string.Empty;
            Cliente ocliente = objDato.Listar().FirstOrDefault(c => c.Correo == correo);

            if (ocliente == null)
            {
                Mensaje = "No se encontró una cuenta de menbresia asociada a este correo.";
                return false;
            }

            string nuevaclave = CN_Recursos.GenerarClave();
            bool resultado = objDato.ReestablecerClave(ocliente.IdCliente, CN_Recursos.ConvertirSha256(nuevaclave), out Mensaje);

            if (resultado)
            {
                string asunto = "🔑 Restauración de Acceso - Lumi Store";

                // Diseño de Correo Estilo Boutique
                StringBuilder html = new StringBuilder();
                html.Append("<div style='font-family: Arial, sans-serif; background-color: #f4f7f6; padding: 40px; text-align: center;'>");
                html.Append("  <div style='max-width: 500px; margin: auto; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 30px rgba(0,0,0,0.1);'>");

                // Header con Color Zafiro
                html.Append("    <div style='background-color: #162e55; padding: 30px;'>");
                html.Append("      <h1 style='color: #ffffff; margin: 0; font-family: Georgia, serif; letter-spacing: 3px;'>LUMI STORE</h1>");
                html.Append("      <p style='color: #ff85a2; margin-top: 5px; font-size: 12px; letter-spacing: 2px;'>JOYERÍA & PERFUMERÍA EXCLUSIVA</p>");
                html.Append("    </div>");

                // Body
                html.Append("    <div style='padding: 40px;'>");
                html.Append("      <h2 style='color: #162e55; margin-bottom: 20px;'>Acceso Restablecido</h2>");
                html.Append("      <p style='color: #555; line-height: 1.6;'>Estimado cliente, hemos procesado su solicitud de recuperación. Para volver a disfrutar de nuestra colección, utilice la siguiente clave temporal:</p>");

                // Caja de la Clave (Fancy Pink Soft)
                html.Append("      <div style='background-color: #fff0f3; border: 1px dashed #ff85a2; padding: 20px; margin: 30px 0; border-radius: 12px;'>");
                html.Append($"        <span style='font-size: 24px; font-weight: bold; color: #162e55; letter-spacing: 4px;'>{nuevaclave}</span>");
                html.Append("      </div>");

                html.Append("      <p style='color: #888; font-size: 12px;'>Le recomendamos cambiar esta clave desde su perfil una vez que haya ingresado al salón.</p>");

                // Botón Esmeralda
                html.Append("      <a href='https://tudominio.com' style='display: inline-block; background-color: #10b981; color: #ffffff; padding: 15px 30px; text-decoration: none; border-radius: 10px; font-weight: bold; margin-top: 20px;'>Ingresar a mi Cuenta</a>");
                html.Append("    </div>");

                // Footer
                html.Append("    <div style='background-color: #f9f9f9; padding: 20px; border-top: 1px solid #eee;'>");
                html.Append("      <p style='color: #aaa; font-size: 11px;'>Este es un mensaje automático, por favor no responda a este correo.</p>");
                html.Append("    </div>");
                html.Append("  </div>");
                html.Append("</div>");

                bool respuesta = CN_Recursos.EnviarCorreo(correo, asunto, html.ToString());

                if (!respuesta) Mensaje = "No pudimos enviar el correo de recuperación, intente más tarde.";
                return respuesta;
            }
            return false;
        }
    }
}