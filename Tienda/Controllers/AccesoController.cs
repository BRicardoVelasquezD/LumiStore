using Entidad;
using Negocio;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace Tienda
{
    public class AccesoController : Controller
    {
        // Organiza sección
        public ActionResult Index() => View();
        public ActionResult Registrar() => View();
        public ActionResult Reestablecer() => View();

        public ActionResult CambiarClave()
        {
            if (TempData["IdCliente"] == null) return RedirectToAction("Index");
            return View();
        }

        [HttpPost]
        public ActionResult Index(string correo, string clave)
        {
            // Organiza sección
            Cliente objCliente = new CN_Cliente().Listar().FirstOrDefault(c =>
                c.Correo == correo &&
                c.Clave == CN_Recursos.ConvertirSha256(clave)
            );

            if (objCliente == null)
            {
                ViewBag.Error = "Correo o contraseña incorrectos";
                return View();
            }

            if (objCliente.Reestablecer)
            {
                TempData["IdCliente"] = objCliente.IdCliente;
                return RedirectToAction("CambiarClave");
            }

            FormsAuthentication.SetAuthCookie(objCliente.Correo, false);
            Session["Cliente"] = objCliente;
            return RedirectToAction("Index", "Tienda");
        }

        [HttpPost]
        public ActionResult Registrar(Cliente objeto)
        {
            int resultado = new CN_Cliente().Registrar(objeto, out string mensaje);

            if (resultado > 0)
            {
                return RedirectToAction("Index");
            }
            else
            {
                // Organiza sección
                ViewBag.Error = mensaje;
                return View(objeto);
            }
        }

        [HttpPost]
        public ActionResult Reestablecer(string correo)
        {
            string mensaje = string.Empty;
            bool respuesta = new CN_Cliente().ReestablecerClave(correo, out mensaje);

            if (respuesta)
            {
                return RedirectToAction("Index", "Acceso");
            }
            else
            {
                ViewBag.Error = mensaje;
                return View();
            }
        }

        [HttpPost]
        public ActionResult CambiarClave(int idCliente, string nuevaClave, string confirmarClave)
        {
            if (nuevaClave != confirmarClave)
            {
                ViewBag.Error = "Las contraseñas no coinciden";
                TempData["IdCliente"] = idCliente;
                return View();
            }

            // Organiza sección
            string claveEncriptada = CN_Recursos.ConvertirSha256(nuevaClave);
            bool respuesta = new CN_Cliente().CambiarClave(idCliente, claveEncriptada, out string mensaje);

            if (respuesta)
            {
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.Error = mensaje;
                TempData["IdCliente"] = idCliente;
                return View();
            }
        }

        public ActionResult CerrarSesion()
        {
            FormsAuthentication.SignOut();
            Session["Cliente"] = null;
            return RedirectToAction("Index");
        }
    }
}