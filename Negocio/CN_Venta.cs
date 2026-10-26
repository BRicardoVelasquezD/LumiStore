using CapaDatos;
using Entidad;
using System.Collections.Generic;
using System.Data;

namespace Negocio
{
    public class CN_Venta
    {
        private CD_Venta objCapaDato = new CD_Venta();

        public bool Registrar(Venta obj, DataTable DetalleVenta, out string Mensaje)
        {
            // Organiza sección
            return objCapaDato.Registrar(obj, DetalleVenta, out Mensaje);
        }

        public List<Venta> ListarVentasPorCliente(int idCliente)
        {
            return objCapaDato.ListarVentasPorCliente(idCliente);
        }

        public List<DetalleVenta> ObtenerDetalleVenta(int idVenta)
        {
            return objCapaDato.ObtenerDetalleVenta(idVenta);
        }
    }
}
