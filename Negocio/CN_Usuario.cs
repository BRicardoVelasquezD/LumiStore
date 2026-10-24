using Datos;
using Entidad;
using System;
using System.Collections.Generic;

namespace Negocio
{
    public class CN_Usuario
    {
        private CD_Usuario objDato = new CD_Usuario();

        public List<Usuario> Listar() => objDato.Listar();

        public int Registrar(Usuario obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrWhiteSpace(obj.Nombres)) Mensaje = "El nombre es obligatorio";
            if (string.IsNullOrWhiteSpace(obj.Apellidos)) Mensaje = "El apellido es obligatorio";
            if (string.IsNullOrWhiteSpace(obj.Correo)) Mensaje = "El correo es obligatorio";

            if (string.IsNullOrEmpty(Mensaje))
            {
                string clave = CN_Recursos.GenerarClave();
                string asunto = "Lumi Store - Acceso Elite";
                string mensaje_correo = $@"
                <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7ff; padding: 40px 0;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 30px rgba(22, 46, 85, 0.1); border: 1px solid #e0e7ff;'>
                        <div style='background: linear-gradient(135deg, #162e55 0%, #1d4d44 100%); padding: 40px; text-align: center;'>
                            <h1 style='color: #ffffff; margin: 0; letter-spacing: 4px; font-weight: 300; font-size: 28px;'>LUMI STORE</h1>
                            <div style='height: 1px; background: rgba(255,133,162,0.4); width: 60px; margin: 15px auto;'></div>
                            <p style='color: #ff85a2; margin: 0; text-transform: uppercase; font-size: 11px; letter-spacing: 4px; font-weight: 600;'>Luxury Experience</p>
                        </div>
                        <div style='padding: 50px 40px; text-align: center;'>
                            <h2 style='color: #162e55; font-size: 24px; margin-top: 0; font-weight: 600;'>¡Felicidades!</h2>
                            <p style='color: #556b8d; font-size: 16px; line-height: 1.8; margin-bottom: 30px;'>Su cuenta exclusiva ha sido activada con éxito. Para iniciar su experiencia, utilice la siguiente clave de acceso temporal:</p>
                            
                            <div style='background-color: #f8faff; border-radius: 12px; padding: 25px; border: 1px dashed #c0ccda; display: inline-block; min-width: 200px;'>
                                <span style='display: block; color: #8492a6; font-size: 12px; text-transform: uppercase; margin-bottom: 8px; letter-spacing: 1px;'>Contraseña Temporal</span>
                                <span style='font-size: 32px; font-weight: bold; color: #162e55; letter-spacing: 2px;'>!clave!</span>
                            </div>

                            <p style='color: #8492a6; font-size: 13px; margin-top: 35px;'>Por seguridad, le recomendamos cambiar su contraseña al ingresar por primera vez.</p>
                        </div>
                        <div style='background-color: #fcfdfe; padding: 25px; text-align: center; border-top: 1px solid #f0f4f8;'>
                            <p style='color: #b0b9c8; font-size: 12px; margin: 0;'>&copy; {DateTime.Now.Year} Lumi Store Elite. Todos los derechos reservados.</p>
                        </div>
                    </div>
                </div>";

                mensaje_correo = mensaje_correo.Replace("!clave!", clave);
                bool correoEnviado = CN_Recursos.EnviarCorreo(obj.Correo, asunto, mensaje_correo);

                if (correoEnviado)
                {
                    obj.Clave = CN_Recursos.ConvertirSha256(clave);
                    return objDato.Registrar(obj, out Mensaje);
                }
                else
                {
                    Mensaje = "Error al enviar el correo de bienvenida.";
                    return 0;
                }
            }
            return 0;
        }

        public bool Editar(Usuario obj, out string Mensaje) => objDato.Editar(obj, out Mensaje);
        public bool Eliminar(int id, out string Mensaje) => objDato.Eliminar(id, out Mensaje);
        public bool CambiarClave(int idusuario, string nuevaclave, out string Mensaje) => objDato.CambiarClave(idusuario, nuevaclave, out Mensaje);

        public bool ReestablecerClave(int idusuario, string correo, out string Mensaje)
        {
            string nuevaclave = CN_Recursos.GenerarClave();
            bool resultado = objDato.ReestablecerClave(idusuario, CN_Recursos.ConvertirSha256(nuevaclave), out Mensaje);

            if (resultado)
            {
                string asunto = "Restablecer Acceso - Lumi Store";

                // Organiza sección
                string mensaje_correo = $@"
                <div style='font-family: ""Segoe UI"", Tahoma, Geneva, Verdana, sans-serif; background-color: #f4f7ff; padding: 40px 0;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 10px 30px rgba(22, 46, 85, 0.1); border: 1px solid #e0e7ff;'>
                        <div style='background: #162e55; padding: 30px; text-align: center;'>
                            <h2 style='color: #ffffff; margin: 0; letter-spacing: 3px; font-weight: 300; font-size: 20px;'>LUMI STORE</h2>
                        </div>
                        <div style='padding: 50px 40px; text-align: center;'>
                            <div style='margin-bottom: 25px;'>
                                <span style='background-color: #fff0f3; color: #ff85a2; padding: 8px 15px; border-radius: 50px; font-size: 12px; font-weight: bold; text-transform: uppercase; letter-spacing: 1px;'>Seguridad de Cuenta</span>
                            </div>
                            <h2 style='color: #162e55; font-size: 22px; margin-top: 0;'>Nueva Clave Generada</h2>
                            <p style='color: #556b8d; font-size: 15px; line-height: 1.7;'>Hemos recibido una solicitud para restablecer su contraseña. A continuación, le proporcionamos una nueva clave temporal:</p>
                            
                            <div style='margin: 30px 0; background-color: #f8faff; border-radius: 12px; padding: 20px; border: 1px solid #e0e7ff;'>
                                <span style='font-size: 28px; font-weight: bold; color: #ff85a2; letter-spacing: 3px;'>{nuevaclave}</span>
                            </div>

                            <p style='color: #8492a6; font-size: 13px;'>Si usted no solicitó este cambio, por favor contacte a soporte técnico de inmediato.</p>
                        </div>
                    </div>
                </div>";

                return CN_Recursos.EnviarCorreo(correo, asunto, mensaje_correo);
            }
            return false;
        }
    }
}