using Entidad;
using Negocio;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace Administrador.Controllers
{
    public class AccesoController : Controller
    {
        // Organiza sección
        public ActionResult Index() => View();
        public ActionResult Reestablecer() => View();
        public ActionResult CambiarClave() => View();

        [HttpPost]
        public ActionResult Index(string correo, string clave)
        {
            Usuario oUsuario = new CN_Usuario().Listar().FirstOrDefault(u =>
                u.Activo &&
                u.Correo == correo &&
                u.Clave == CN_Recursos.ConvertirSha256(clave));

            if (oUsuario == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }
            if (oUsuario.Reestablecer)
            {
                TempData["IdUsuario"] = oUsuario.IdUsuario;
                return RedirectToAction("CambiarClave");
            }

            FormsAuthentication.SetAuthCookie(oUsuario.Correo, false);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult CambiarClave(string idusuario, string claveactual, string nuevaclave, string confirmarclave)
        {
            Usuario oUsuario = new CN_Usuario().Listar().Where(u => u.IdUsuario == int.Parse(idusuario)).FirstOrDefault();

            if (oUsuario.Clave != CN_Recursos.ConvertirSha256(claveactual))
            {
                TempData["IdUsuario"] = idusuario;
                ViewBag.Error = "La clave actual es incorrecta";
                return View();
            }
            if (nuevaclave != confirmarclave)
            {
                TempData["IdUsuario"] = idusuario;
                ViewBag.Error = "Las claves no coinciden";
                return View();
            }

            string mensaje = string.Empty;
            bool respuesta = new CN_Usuario().CambiarClave(int.Parse(idusuario), CN_Recursos.ConvertirSha256(nuevaclave), out mensaje);

            if (respuesta) return RedirectToAction("Index");

            TempData["IdUsuario"] = idusuario;
            ViewBag.Error = mensaje;
            return View();
        }

        [HttpPost]
        public ActionResult Reestablecer(string correo)
        {
            Usuario ousuario = new CN_Usuario().Listar().Where(u => u.Correo == correo).FirstOrDefault();
            if (ousuario == null)
            {
                ViewBag.Error = "No se encontró el correo";
                return View();
            }

            string mensaje = string.Empty;
            if (new CN_Usuario().ReestablecerClave(ousuario.IdUsuario, correo, out mensaje))
            {
                TempData["MensajeExito"] = "Clave enviada correctamente.";
                return RedirectToAction("Index");
            }
            ViewBag.Error = mensaje;
            return View();
        }

        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
    }
}
