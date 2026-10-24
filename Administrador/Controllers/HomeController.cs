using ClosedXML.Excel;
using Entidad;
using Negocio;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web.Mvc;

namespace Administrador.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Usuarios()
        {
            return View();
        }

        #region DASHBOARD Y REPORTES

        [HttpGet]
        public JsonResult VistaDashboard()
        {
            Dashboard objeto = new CN_Reporte().VerDashboard();
            return Json(new { resultado = objeto }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ListarReporte(string fechainicio, string fechafin, string idtransaccion)
        {
            List<Reporte> oLista = new List<Reporte>();
            oLista = new CN_Reporte().Ventas(fechainicio, fechafin, idtransaccion);
            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public FileResult ExportarVenta(string fechainicio, string fechafin, string idtransaccion)
        {
            // Organiza sección
            List<Reporte> oLista = new CN_Reporte().Ventas(fechainicio, fechafin, idtransaccion);

            // Organiza sección
            DataTable dt = new DataTable();
            dt.TableName = "Reporte";
            dt.Columns.Add("Fecha Venta", typeof(string));
            dt.Columns.Add("Cliente", typeof(string));
            dt.Columns.Add("Producto", typeof(string));
            dt.Columns.Add("Precio", typeof(decimal));
            dt.Columns.Add("Cantidad", typeof(string));
            dt.Columns.Add("Total", typeof(decimal));
            dt.Columns.Add("ID Transaccion", typeof(string));

            foreach (Reporte rp in oLista)
            {
                dt.Rows.Add(new object[] {
    rp.FechaVenta,
    rp.Cliente,
    rp.Producto,
    Convert.ToDecimal(rp.Precio),
    rp.Cantidad,
    Convert.ToDecimal(rp.Total),
    rp.IdTransaccion
});
            }

            // Organiza sección
            using (XLWorkbook wb = new XLWorkbook())
            {
                var hoja = wb.Worksheets.Add(dt);
                hoja.ColumnsUsed().AdjustToContents();

                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    string nombreArchivo = "Reporte_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                    // Organiza sección
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
                }
            }
        }

        #endregion

        #region MANTENIMIENTO USUARIOS

        [HttpGet]
        public JsonResult ListarUsuarios()
        {
            List<Usuario> oLista = new CN_Usuario().Listar();
            return Json(new { data = oLista }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult GuardarUsuario(Usuario objeto)
        {
            object resultado;
            string mensaje = string.Empty;

            if (objeto.IdUsuario == 0)
            {
                resultado = new CN_Usuario().Registrar(objeto, out mensaje);
            }
            else
            {
                resultado = new CN_Usuario().Editar(objeto, out mensaje);
            }

            return Json(new { resultado = resultado, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EliminarUsuario(int id)
        {
            bool respuesta = false;
            string mensaje = string.Empty;
            respuesta = new CN_Usuario().Eliminar(id, out mensaje);
            return Json(new { resultado = respuesta, mensaje = mensaje }, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}