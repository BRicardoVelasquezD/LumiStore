using Datos;
using Entidad;
using System.Collections.Generic;

namespace Negocio
{
    public class CN_Reporte
    {
        private CD_Reporte objDato = new CD_Reporte();

        public Dashboard VerDashboard()
        {
            return objDato.VerDashboard();
        }

        public List<Reporte> Ventas(string fechainicio, string fechafin, string idtransaccion)
        {
            return objDato.Ventas(fechainicio, fechafin, idtransaccion);
        }
    }
}
