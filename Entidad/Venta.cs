using System.Collections.Generic;

namespace Entidad
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public int IdCliente { get; set; }
        public decimal TotalProducto { get; set; }
        public string Contacto { get; set; }
        public string IdCiudad { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string FechaTexto { get; set; }
        public string IdTransaccion { get; set; }
        public List<DetalleVenta> oDetalleVenta { get; set; }
        public object MontoTotal { get; set; }
    }
}
