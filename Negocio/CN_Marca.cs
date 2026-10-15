using Datos;
using Entidad;
using System.Collections.Generic;

namespace Negocio
{
    public class CN_Marca
    {
        private CD_Marca objDato = new CD_Marca();

        public List<Marca> Listar() => objDato.Listar();
        public List<Marca> ListarMarcasPorCategoria(int idcategoria) => objDato.ListarMarcaporCategoria(idcategoria);

        public int Registrar(Marca obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrWhiteSpace(obj.Descripcion)) Mensaje = "La descripción es obligatoria";

            if (string.IsNullOrEmpty(Mensaje)) return objDato.Registrar(obj, out Mensaje);
            return 0;
        }

        public bool Editar(Marca obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrWhiteSpace(obj.Descripcion)) Mensaje = "La descripción es obligatoria";

            if (string.IsNullOrEmpty(Mensaje)) return objDato.Editar(obj, out Mensaje);
            return false;
        }

        public bool Eliminar(int id, out string Mensaje) => objDato.Eliminar(id, out Mensaje);
    }

}