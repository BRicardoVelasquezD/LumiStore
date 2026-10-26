using Datos;
using Entidad;
using Negocio;
using Newtonsoft.Json.Linq;
using Tienda.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Tienda.Controllers
{
    public class TiendaController : Controller
    {
        private static readonly HttpClient FxHttp = new HttpClient { Timeout = TimeSpan.FromSeconds(12) };

        // Organiza sección
        public ActionResult Index()
        {
            return View();
        }

        // Organiza sección
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
                string rutaCompleta = RutaImagenProducto(oProducto);
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
                Base64 = negocio.ConvertirBase64(RutaImagenProducto(p), out bool ok),
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

        // Organiza sección

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
                        oC.oProducto.Base64 = CN_Recursos.ConvertirBase64(RutaImagenProducto(oC.oProducto), out conversion);
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

        // Organiza sección

        [HttpPost]
        public JsonResult ObtenerDepartamento()
        {
            List<Departamento> lista = new List<Departamento>();

            try
            {
                using (SqlConnection oconexion = Conexion.CrearConexion())
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
                // Organiza sección
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

                using (SqlConnection oconexion = Conexion.CrearConexion())
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

                using (SqlConnection oconexion = Conexion.CrearConexion())
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> ProcesarCompra(Venta venta)
        {
            if (Session["Cliente"] == null)
                return Json(new { respuesta = false, mensaje = "Debe iniciar sesión" });

            try
            {
                var cliente = (Cliente)Session["Cliente"];
                var carrito = new CN_Carrito().ListarProducto(cliente.IdCliente);
                if (carrito == null || carrito.Count == 0)
                    return Json(new { respuesta = false, mensaje = "El carrito está vacío." });
                if (venta == null || string.IsNullOrWhiteSpace(venta.IdCiudad) || string.IsNullOrWhiteSpace(venta.Telefono) || string.IsNullOrWhiteSpace(venta.Direccion))
                    return Json(new { respuesta = false, mensaje = "Completa los datos de entrega." });

                var pending = new PendingPaypalOrder
                {
                    CustomerId = cliente.IdCliente,
                    IdCiudad = venta.IdCiudad,
                    Telefono = venta.Telefono.Trim(),
                    Direccion = venta.Direccion.Trim(),
                    Contacto = (cliente.Nombres + " " + cliente.Apellidos).Trim(),
                    Lines = carrito.Select(item => new PendingPaypalLine
                    {
                        ProductId = item.oProducto.IdProducto,
                        Quantity = item.Cantidad,
                        UnitPrice = item.oProducto.Precio
                    }).ToList()
                };

                decimal subtotal = pending.Lines.Sum(item => item.UnitPrice * item.Quantity);
                decimal envio = subtotal >= 500000m ? 0m : 15000m;
                pending.TotalCop = decimal.Round(subtotal + decimal.Round(subtotal * 0.19m, 2, MidpointRounding.AwayFromZero) + envio, 2, MidpointRounding.AwayFromZero);
                pending.CopPerUsd = await ObtenerTasaCopPorUsdAsync();
                pending.TotalUsd = decimal.Round(pending.TotalCop / pending.CopPerUsd, 2, MidpointRounding.AwayFromZero);
                if (pending.TotalUsd <= 0m)
                    throw new InvalidOperationException("El total convertido no es válido.");

                var gateway = new PaypalGateway();
                string returnUrl = Url.Action("PaypalRetorno", "Tienda", null, Request.Url.Scheme);
                string cancelUrl = Url.Action("PaypalCancelar", "Tienda", null, Request.Url.Scheme);
                var order = await gateway.CreateOrderAsync(pending.TotalUsd, returnUrl, cancelUrl);
                pending.OrderId = order.Item1;
                Session["PaypalPendingOrder"] = pending;

                return Json(new
                {
                    respuesta = true,
                    approvalUrl = order.Item2,
                    totalCop = pending.TotalCop,
                    totalUsd = pending.TotalUsd.ToString("0.00", CultureInfo.InvariantCulture),
                    copPerUsd = pending.CopPerUsd
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ProcesarCompra: " + ex.Message);
                Response.StatusCode = 503;
                return Json(new { respuesta = false, mensaje = "No se pudo iniciar el pago. Revisa la configuración sandbox de PayPal y la tasa USD/COP." });
            }
        }

        [HttpGet]
        public async Task<ActionResult> PaypalRetorno(string token)
        {
            var pending = Session["PaypalPendingOrder"] as PendingPaypalOrder;
            if (Session["Cliente"] == null || pending == null || string.IsNullOrWhiteSpace(token) || token != pending.OrderId)
                return MostrarResultadoPago(false, "No encontramos una compra pendiente para confirmar.");

            try
            {
                var cliente = (Cliente)Session["Cliente"];
                if (cliente.IdCliente != pending.CustomerId || !CarritoCoincide(pending))
                    return MostrarResultadoPago(false, "El carrito cambió durante el pago. No se capturó el pedido.");

                var gateway = new PaypalGateway();
                JObject capture = await gateway.CaptureOrderAsync(pending.OrderId);
                JToken pago = capture["purchase_units"]?.FirstOrDefault()?["payments"]?["captures"]?.FirstOrDefault();
                string captureId = (string)pago?["id"];
                string estado = (string)pago?["status"];
                string moneda = (string)pago?["amount"]?["currency_code"];
                decimal amountUsd;
                if (!string.Equals((string)capture["status"], "COMPLETED", StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(estado, "COMPLETED", StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(moneda, "USD", StringComparison.OrdinalIgnoreCase) ||
                    !decimal.TryParse((string)pago?["amount"]?["value"], NumberStyles.Number, CultureInfo.InvariantCulture, out amountUsd) ||
                    amountUsd != pending.TotalUsd || string.IsNullOrWhiteSpace(captureId))
                    return MostrarResultadoPago(false, "PayPal no confirmó el monto esperado en USD.");

                DataTable detalle = new DataTable();
                detalle.Columns.Add("IdProducto", typeof(int));
                detalle.Columns.Add("Cantidad", typeof(int));
                detalle.Columns.Add("Total", typeof(decimal));
                foreach (var linea in pending.Lines)
                    detalle.Rows.Add(linea.ProductId, linea.Quantity, decimal.Round(linea.UnitPrice * linea.Quantity, 2, MidpointRounding.AwayFromZero));

                var venta = new Venta
                {
                    IdCliente = pending.CustomerId,
                    TotalProducto = pending.Lines.Sum(linea => linea.Quantity),
                    MontoTotal = pending.TotalCop,
                    Contacto = pending.Contacto,
                    IdCiudad = pending.IdCiudad,
                    Telefono = pending.Telefono,
                    Direccion = pending.Direccion,
                    IdTransaccion = captureId
                };

                if (!new CN_Venta().Registrar(venta, detalle, out string mensaje))
                {
                    bool reembolso = await gateway.RefundCaptureAsync(captureId);
                    Session.Remove("PaypalPendingOrder");
                    return MostrarResultadoPago(false, reembolso
                        ? "El inventario cambió después del pago. PayPal inició el reembolso automático."
                        : "PayPal confirmó el pago, pero no se pudo guardar la venta ni confirmar el reembolso. Contacta a soporte con el código " + captureId + ".");
                }

                Session.Remove("PaypalPendingOrder");
                TempData["CompraNumero"] = captureId;
                TempData["CompraTotalCop"] = pending.TotalCop.ToString("C0", CultureInfo.GetCultureInfo("es-CO"));
                TempData["CompraTotalUsd"] = pending.TotalUsd.ToString("C2", CultureInfo.GetCultureInfo("en-US"));
                TempData["CompraFecha"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                return RedirectToAction("CompraConfirmada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("PaypalRetorno: " + ex.Message);
                return MostrarResultadoPago(false, "No fue posible confirmar el pago. Revisa el estado de la operación en PayPal antes de volver a intentarlo.");
            }
        }

        [HttpGet]
        public ActionResult PaypalCancelar()
        {
            Session.Remove("PaypalPendingOrder");
            return MostrarResultadoPago(false, "Cancelaste el pago. No se registró la compra.");
        }

        [HttpGet]
        public ActionResult CompraConfirmada()
        {
            if (TempData["CompraNumero"] == null)
                return RedirectToAction("Index");
            ViewBag.Numero = TempData["CompraNumero"];
            ViewBag.TotalCop = TempData["CompraTotalCop"];
            ViewBag.TotalUsd = TempData["CompraTotalUsd"];
            ViewBag.Fecha = TempData["CompraFecha"];
            return View();
        }

        private ActionResult MostrarResultadoPago(bool exito, string mensaje)
        {
            ViewBag.Exito = exito;
            ViewBag.Mensaje = mensaje;
            return View("PagoResultado");
        }

        private bool CarritoCoincide(PendingPaypalOrder pending)
        {
            var actual = new CN_Carrito().ListarProducto(pending.CustomerId);
            if (actual == null || actual.Count != pending.Lines.Count)
                return false;
            var lineas = actual.OrderBy(item => item.oProducto.IdProducto).ToList();
            var esperadas = pending.Lines.OrderBy(item => item.ProductId).ToList();
            for (int i = 0; i < lineas.Count; i++)
            {
                if (lineas[i].oProducto.IdProducto != esperadas[i].ProductId ||
                    lineas[i].Cantidad != esperadas[i].Quantity ||
                    lineas[i].oProducto.Precio != esperadas[i].UnitPrice)
                    return false;
            }
            return true;
        }

        private string RutaImagenProducto(Producto producto)
        {
            string nombre = Path.GetFileName(producto?.NombreImagen ?? string.Empty);
            if (string.IsNullOrWhiteSpace(nombre))
                return string.Empty;

            string rutaLocal = Server.MapPath("~/Imagenes/Productos/" + nombre);
            if (System.IO.File.Exists(rutaLocal))
                return rutaLocal;

            if (!string.IsNullOrWhiteSpace(producto.RutaImagen))
            {
                string rutaOriginal = Path.Combine(producto.RutaImagen, nombre);
                if (System.IO.File.Exists(rutaOriginal))
                    return rutaOriginal;
            }

            return rutaLocal;
        }

        private static async Task<decimal> ObtenerTasaCopPorUsdAsync()
        {
            try
            {
                using (HttpResponseMessage response = await FxHttp.GetAsync("https://open.er-api.com/v6/latest/USD"))
                {
                    response.EnsureSuccessStatusCode();
                    JObject json = JObject.Parse(await response.Content.ReadAsStringAsync());
                    decimal rate = (decimal?)json["rates"]?["COP"] ?? 0m;
                    if (string.Equals((string)json["result"], "success", StringComparison.OrdinalIgnoreCase) && rate > 0m)
                        return rate;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ObtenerTasaCopPorUsd: " + ex.Message);
            }

            string fallback = Environment.GetEnvironmentVariable("LUMISTORE_COP_PER_USD");
            if (decimal.TryParse(fallback, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal configuredRate) && configuredRate > 0m)
                return configuredRate;
            throw new InvalidOperationException("No hay una tasa USD/COP disponible.");
        }

        [Serializable]
        private sealed class PendingPaypalOrder
        {
            public int CustomerId { get; set; }
            public string OrderId { get; set; }
            public string Contacto { get; set; }
            public string IdCiudad { get; set; }
            public string Telefono { get; set; }
            public string Direccion { get; set; }
            public decimal TotalCop { get; set; }
            public decimal TotalUsd { get; set; }
            public decimal CopPerUsd { get; set; }
            public List<PendingPaypalLine> Lines { get; set; }
        }

        [Serializable]
        private sealed class PendingPaypalLine
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
        }
    }
}
