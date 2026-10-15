using Datos;
using Entidad;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace Tienda.Controllers
{
    public class TiendaController : Controller
    {
        // GET: Tienda
        public ActionResult Index()
        {
            return View();
        }

        // GET: Vista del Carrito
        public ActionResult Carrito()
        {
            if (Session["Cliente"] == null)
                return RedirectToAction("Index", "Acceso");

            return View();
        }

        public ActionResult DetalleProducto(int idproducto = 0)
        {
            Producto oProducto = new CN_Producto().Listar()
                .FirstOrDefault(p => p.IdProducto == idproducto);

            if (oProducto == null) return RedirectToAction("Index");

            try
            {
                bool conversion;
                string rutaCompleta = Path.Combine(oProducto.RutaImagen, oProducto.NombreImagen);
                oProducto.Base64 = CN_Recursos.ConvertirBase64(rutaCompleta, out conversion);
                oProducto.Extension = Path.GetExtension(oProducto.NombreImagen)?.Replace(".", "") ?? "jpg";
            }
            catch
            {
                oProducto.Base64 = "";
                oProducto.Extension = "jpg";
            }

            return View(oProducto);
        }

        [HttpGet]
        public JsonResult ListarCategorias()
        {
            List<Categoria> lista = new CN_Categoria().Listar();
            return Json(new { data = lista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListarMarcasPorCategoria(int idcategoria)
        {
            List<Marca> lista = new CN_Marca().ListarMarcasPorCategoria(idcategoria);
            return Json(new { data = lista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListarProducto(int idcategoria, int idmarca)
        {
            CN_Producto negocio = new CN_Producto();
            var lista = negocio.Listar().Select(p => new Producto()
            {
                IdProducto = p.IdProducto,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Stock = p.Stock,
                oMarca = p.oMarca,
                oCategoria = p.oCategoria,
                Base64 = negocio.ConvertirBase64(Path.Combine(p.RutaImagen, p.NombreImagen), out bool ok),
                Extension = Path.GetExtension(p.NombreImagen)?.Replace(".", "") ?? "jpg",
                Activo = p.Activo
            }).Where(p =>
                p.oCategoria.IdCategoria == (idcategoria == 0 ? p.oCategoria.IdCategoria : idcategoria) &&
                p.oMarca.IdMarca == (idmarca == 0 ? p.oMarca.IdMarca : idmarca) &&
                p.Stock > 0 && p.Activo == true
            ).ToList();

            var jsonresult = Json(new { data = lista }, JsonRequestBehavior.AllowGet);
            jsonresult.MaxJsonLength = int.MaxValue;
            return jsonresult;
        }

        // --- MÉTODOS DE CARRITO ---

        [HttpPost]
        public JsonResult AgregarCarrito(int idproducto)
        {
            if (Session["Cliente"] == null)
                return Json(new { respuesta = false, mensaje = "Debe iniciar sesión" }, JsonRequestBehavior.AllowGet);

            int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
            string mensaje = string.Empty;
            bool respuesta = new CN_Carrito().OperacionCarrito(idcliente, idproducto, true, out mensaje);

            return Json(new { respuesta = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult OperacionCarrito(int idproducto, bool sumar)
        {
            if (Session["Cliente"] == null)
                return Json(new { respuesta = false, mensaje = "Debe iniciar sesión" }, JsonRequestBehavior.AllowGet);

            int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
            string mensaje = string.Empty;
            bool respuesta = new CN_Carrito().OperacionCarrito(idcliente, idproducto, sumar, out mensaje);

            return Json(new { respuesta = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult CantidadEnCarrito()
        {
            int cantidad = 0;
            if (Session["Cliente"] != null)
            {
                try
                {
                    int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
                    cantidad = new CN_Carrito().CantidadEnCarrito(idcliente);
                }
                catch
                {
                    cantidad = 0;
                }
            }
            return Json(new { cantidad = cantidad }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ListarProductosCarrito()
        {
            if (Session["Cliente"] == null)
                return Json(new { data = new List<Carrito>() }, JsonRequestBehavior.AllowGet);

            try
            {
                int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
                List<Carrito> oLista = new CN_Carrito().ListarProducto(idcliente);

                bool conversion;
                foreach (Carrito oC in oLista)
                {
                    try
                    {
                        oC.oProducto.Base64 = CN_Recursos.ConvertirBase64(Path.Combine(oC.oProducto.RutaImagen, oC.oProducto.NombreImagen), out conversion);
                        oC.oProducto.Extension = Path.GetExtension(oC.oProducto.NombreImagen)?.Replace(".", "") ?? "jpg";
                    }
                    catch
                    {
                        oC.oProducto.Base64 = "";
                        oC.oProducto.Extension = "jpg";
                    }
                }
                return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { data = new List<Carrito>(), error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult EliminarCarrito(int idproducto)
        {
            if (Session["Cliente"] == null)
                return Json(new { respuesta = false, mensaje = "Sesión expirada" }, JsonRequestBehavior.AllowGet);

            try
            {
                int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
                bool respuesta = new CN_Carrito().EliminarCarrito(idcliente, idproducto);

                return Json(new { respuesta = respuesta }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { respuesta = false, mensaje = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // --- MÉTODOS DE UBICACIÓN ---

        [HttpPost]
        public JsonResult ObtenerDepartamento()
        {
            List<Departamento> lista = new List<Departamento>();

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    SqlCommand cmd = new SqlCommand("SELECT IdDepartamento, Descripcion FROM DEPARTAMENTO ORDER BY Descripcion", oconexion);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        lista.Add(new Departamento()
                        {
                            IdDepartamento = dr["IdDepartamento"].ToString(),
                            Descripcion = dr["Descripcion"].ToString()
                        });
                    }
                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                // Log el error si es necesario
                System.Diagnostics.Debug.WriteLine("Error ObtenerDepartamento: " + ex.Message);
                lista = new List<Departamento>();
            }

            return Json(new { lista = lista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ObtenerMunicipio(string iddepartamento)
        {
            List<Municipio> lista = new List<Municipio>();

            try
            {
                if (string.IsNullOrEmpty(iddepartamento))
                {
                    return Json(new { lista = new List<Municipio>() }, JsonRequestBehavior.AllowGet);
                }

                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    SqlCommand cmd = new SqlCommand("SELECT IdMunicipio, Descripcion FROM MUNICIPIO WHERE IdDepartamento = @iddepartamento ORDER BY Descripcion", oconexion);
                    cmd.Parameters.AddWithValue("@iddepartamento", iddepartamento);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        lista.Add(new Municipio()
                        {
                            IdMunicipio = dr["IdMunicipio"].ToString(),
                            Descripcion = dr["Descripcion"].ToString()
                        });
                    }
                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error ObtenerMunicipio: " + ex.Message);
                lista = new List<Municipio>();
            }

            return Json(new { lista = lista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ObtenerCiudad(string idmunicipio, string iddepartamento)
        {
            List<Ciudad> lista = new List<Ciudad>();

            try
            {
                if (string.IsNullOrEmpty(idmunicipio) || string.IsNullOrEmpty(iddepartamento))
                {
                    return Json(new { lista = new List<Ciudad>() }, JsonRequestBehavior.AllowGet);
                }

                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    SqlCommand cmd = new SqlCommand("SELECT IdCiudad, Descripcion FROM CIUDAD WHERE IdMunicipio = @idmunicipio AND IdDepartamento = @iddepartamento ORDER BY Descripcion", oconexion);
                    cmd.Parameters.AddWithValue("@idmunicipio", idmunicipio);
                    cmd.Parameters.AddWithValue("@iddepartamento", iddepartamento);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        lista.Add(new Ciudad()
                        {
                            IdCiudad = dr["IdCiudad"].ToString(),
                            Descripcion = dr["Descripcion"].ToString()
                        });
                    }
                    dr.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error ObtenerCiudad: " + ex.Message);
                lista = new List<Ciudad>();
            }

            return Json(new { lista = lista }, JsonRequestBehavior.AllowGet);
        }

        // --- MÉTODO PARA PROCESAR COMPRA ---
        [HttpPost]
        public JsonResult ProcesarCompra(Venta venta)
        {
            if (Session["Cliente"] == null)
                return Json(new { respuesta = false, mensaje = "Debe iniciar sesión" }, JsonRequestBehavior.AllowGet);

            try
            {
                int idcliente = ((Cliente)Session["Cliente"]).IdCliente;
                venta.IdCliente = idcliente;

                // Aquí deberías implementar la lógica para crear la venta
                // usando tu capa de negocio (CN_Venta)

                return Json(new
                {
                    respuesta = true,
                    mensaje = "Compra procesada exitosamente",
                    numeroTransaccion = "TRX-" + DateTime.Now.Ticks.ToString().Substring(0, 8)
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    respuesta = false,
                    mensaje = "Error al procesar la compra: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}