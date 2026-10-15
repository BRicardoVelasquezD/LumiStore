using Datos;
using Entidad;
using System.Collections.Generic;

namespace Negocio
{
    public class CN_Carrito
    {
        private CD_Carrito objDato = new CD_Carrito();

        // Métodos relacionados con el carrito
        public List<Carrito> ListarProducto(int idcliente)
            => objDato.ListarProducto(idcliente);

        public bool ExisteCarrito(int idcliente, int idproducto)
            => objDato.ExisteCarrito(idcliente, idproducto);

        public bool OperacionCarrito(int idcliente, int idproducto, bool sumar, out string Mensaje)
            => objDato.OperacionCarrito(idcliente, idproducto, sumar, out Mensaje);

        public int CantidadEnCarrito(int idcliente)
            => objDato.CantidadEnCarrito(idcliente);

        public bool EliminarCarrito(int idcliente, int idproducto)
            => objDato.EliminarCarrito(idcliente, idproducto);

    }
}