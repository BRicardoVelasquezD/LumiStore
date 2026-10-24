using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace Negocio
{
    public class CN_Recursos
    {
        public static string GenerarClave()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 6);
        }

        public static string ConvertirSha256(string texto)
        {
            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));
                foreach (byte b in result) Sb.Append(b.ToString("x2"));
            }
            return Sb.ToString();
        }

        public static bool EnviarCorreo(string correo, string asunto, string mensaje)
        {
            string usuario = Environment.GetEnvironmentVariable("LUMISTORE_SMTP_USERNAME");
            string clave = Environment.GetEnvironmentVariable("LUMISTORE_SMTP_PASSWORD");
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
                return false;

            try
            {
                using (MailMessage mail = new MailMessage())
                using (var smtp = new SmtpClient())
                {
                    mail.To.Add(correo);
                    mail.From = new MailAddress(ConfigurationManager.AppSettings["SmtpFrom"] ?? "brayanvelasquez728@gmail.com");
                    mail.Subject = asunto;
                    mail.Body = mensaje;
                    mail.IsBodyHtml = true;
                    smtp.Credentials = new NetworkCredential(usuario, clave);
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
                return true;
            }
            catch { return false; }
        }

        public static string ConvertirBase64(string rutaCompleta, out bool conversion)
        {
            string textoBase64 = string.Empty;
            conversion = true;

            try
            {
                byte[] bytes = File.ReadAllBytes(rutaCompleta);

                textoBase64 = Convert.ToBase64String(bytes);
            }
            catch (Exception)
            {
                conversion = false;
            }

            return textoBase64;
        }
    }
}
