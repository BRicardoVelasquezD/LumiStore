using Entidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class CD_Reporte
    {
        public Dashboard VerDashboard()
        {
            Dashboard objeto = new Dashboard();
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    SqlCommand cmd = new SqlCommand("sp_ReporteDashboard", oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            objeto.TotalCliente = Convert.ToInt32(dr["TotalCliente"]);
                            objeto.TotalVenta = Convert.ToInt32(dr["TotalVenta"]);
                            objeto.TotalProducto = Convert.ToInt32(dr["TotalProducto"]);
                        }
                    }
                }
            }
            catch { objeto = new Dashboard(); }
            return objeto;
        }

        public List<Reporte> Ventas(string fechainicio, string fechafin, string idtransaccion)
        {
            List<Reporte> lista = new List<Reporte>();
            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    // Organiza sección
                    SqlCommand cmd = new SqlCommand("sp_ReporteVentas", oconexion);
                    cmd.Parameters.AddWithValue("fechainicio", fechainicio);
                    cmd.Parameters.AddWithValue("fechafin", fechafin);
                    cmd.Parameters.AddWithValue("idtransaccion", idtransaccion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Reporte()
                            {
                                FechaVenta = dr["FechaVenta"].ToString(),
                                Cliente = dr["Cliente"].ToString(),
                                Producto = dr["Producto"].ToString(),
                                Precio = dr["Precio"].ToString(),
                                Cantidad = dr["Cantidad"].ToString(),
                                Total = dr["Total"].ToString(),
                                IdTransaccion = dr["IdTransaccion"].ToString()
                            });
                        }
                    }
                }
            }
            catch { lista = new List<Reporte>(); }
            return lista;
        }
    }
}