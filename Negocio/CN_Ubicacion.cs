using Datos;
using Entidad;
using System.Collections.Generic;

namespace Negocio
{
    public class CN_Ubicacion
    {
        private CD_Ubicacion objDato = new CD_Ubicacion();

        public List<Departamento> ObtenerDepartamento()
            => objDato.ObtenerDepartamento();

        public List<Municipio> ObtenerMunicipio(string iddep)
            => objDato.ObtenerMunicipio(iddep);

        public List<Ciudad> ObtenerCiudad(string idmun, string iddep)
            => objDato.ObtenerCiudad(idmun, iddep);
    }
}