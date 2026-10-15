using Datos;
using Entidad;
using System.Collections.Generic;

namespace Negocio
{
    public class CN_Categoria
    {
        private CD_Categoria objDato = new CD_Categoria();

        public List<Categoria> Listar() => objDato.Listar();

        public int Registrar(Categoria obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrWhiteSpace(obj.Descripcion))
                Mensaje = "La descripción de la categoría no puede estar vacía";

            if (string.IsNullOrEmpty(Mensaje))
                return objDato.Registrar(obj, out Mensaje);
            else
                return 0;
        }

        public bool Editar(Categoria obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrWhiteSpace(obj.Descripcion))
                Mensaje = "La descripción de la categoría no puede estar vacía";

            if (string.IsNullOrEmpty(Mensaje))
                return objDato.Editar(obj, out Mensaje);
            else
                return false;
        }

        public bool Eliminar(int id, out string Mensaje) => objDato.Eliminar(id, out Mensaje);
    }
}
