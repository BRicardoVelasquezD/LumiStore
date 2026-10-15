using CapaDatos;
using Entidad;
using System.Collections.Generic;
using System.Data;

namespace CapaNegocio
{
    public class CN_Venta
    {
        private CD_Venta objCapaDato = new CD_Venta();

        public bool Registrar(Venta obj, DataTable DetalleVenta, out string Mensaje)
        {
            // Validaciones de negocio si las necesitas
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
