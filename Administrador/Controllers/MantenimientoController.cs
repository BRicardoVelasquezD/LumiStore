using Entidad;
using Negocio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Administrador.Controllers
{
    [Authorize]
    public class MantenimientoController : Controller
    {
        // GET: Mantenimiento
        public ActionResult Categoria() => View();
        #region Categorias
        [HttpGet]
        public JsonResult ListarCategorias()
        {
            List<Categoria> oLista = new CN_Categoria().Listar();
            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarCategoria(Categoria objeto)
        {
            object resultado;
            string mensaje = string.Empty;

            if (objeto.IdCategoria == 0)
                resultado = new CN_Categoria().Registrar(objeto, out mensaje);
            else
                resultado = new CN_Categoria().Editar(objeto, out mensaje);

            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EliminarCategoria(int id)
        {
            bool respuesta = false;
            string mensaje = string.Empty;
            respuesta = new CN_Categoria().Eliminar(id, out mensaje);
            return Json(new { resultado = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Marcas
        public ActionResult Marca()
        {
            return View();
        }
        [HttpGet]
        public JsonResult ListarMarcas()
        {
            List<Marca> oLista = new CN_Marca().Listar();
            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarMarca(Marca objeto)
        {
            object resultado;
            string mensaje = string.Empty;

            if (objeto.IdMarca == 0)
                resultado = new CN_Marca().Registrar(objeto, out mensaje);
            else
                resultado = new CN_Marca().Editar(objeto, out mensaje);

            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EliminarMarca(int id)
        {
            bool respuesta = false;
            string mensaje = string.Empty;
            respuesta = new CN_Marca().Eliminar(id, out mensaje);
            return Json(new { resultado = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Productos
        public ActionResult Producto()
        {
            return View();
        }

        [HttpGet]
        public JsonResult ListarProducto()
        {
            List<Producto> oLista = new CN_Producto().Listar();
            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarProducto(string objeto, HttpPostedFileBase archivo)
        {
            string mensaje = string.Empty;
            bool operacion_exitosa = true;
            Producto oProducto = Newtonsoft.Json.JsonConvert.DeserializeObject<Producto>(objeto);

            if (oProducto.IdProducto == 0)
            {
                int idGenerado = new CN_Producto().Registrar(oProducto, out mensaje);
                if (idGenerado != 0) oProducto.IdProducto = idGenerado;
                else operacion_exitosa = false;
            }
            else
            {
                operacion_exitosa = new CN_Producto().Editar(oProducto, out mensaje);
            }

            if (operacion_exitosa && archivo != null)
            {
                string rutapath = Server.MapPath("~/Imagenes/Productos/");
                if (!Directory.Exists(rutapath)) Directory.CreateDirectory(rutapath);

                string extension = Path.GetExtension(archivo.FileName);
                string nombre_imagen = string.Concat(oProducto.IdProducto.ToString(), extension);

                try
                {
                    archivo.SaveAs(Path.Combine(rutapath, nombre_imagen));
                    oProducto.RutaImagen = rutapath;
                    oProducto.NombreImagen = nombre_imagen;
                    new CN_Producto().ActualizarRutaImagen(oProducto, out mensaje);
                }
                catch (Exception ex) { mensaje = ex.Message; }
            }
            return Json(new { operacion_exitosa = operacion_exitosa, idGenerado = oProducto.IdProducto, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult MostrarImagen(int id)
        {
            bool conversion;
            Producto oProducto = new CN_Producto().Listar().FirstOrDefault(p => p.IdProducto == id);
            string textoBase64 = new CN_Producto().ConvertirBase64(Path.Combine(oProducto.RutaImagen, oProducto.NombreImagen), out conversion);

            return Json(new
            {
                conversion = conversion,
                textoBase64 = textoBase64,
                extension = Path.GetExtension(oProducto.NombreImagen)
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EliminarProducto(int id)
        {
            bool respuesta = new CN_Producto().Eliminar(id, out string mensaje);
            return Json(new { resultado = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}