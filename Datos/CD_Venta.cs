using Datos;
using Entidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace CapaDatos
{
    public class CD_Venta
    {
        public bool Registrar(Venta obj, DataTable DetalleVenta, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                using (SqlCommand cmd = new SqlCommand("usp_RegistrarVenta", oconexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@IdCliente", obj.IdCliente);
                    cmd.Parameters.AddWithValue("@TotalProducto", obj.TotalProducto);
                    cmd.Parameters.AddWithValue("@MontoTotal", obj.MontoTotal);
                    cmd.Parameters.AddWithValue("@Contacto", (object)obj.Contacto ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdCiudad", (object)obj.IdCiudad ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Telefono", (object)obj.Telefono ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Direccion", (object)obj.Direccion ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@IdTransaccion", (object)obj.IdTransaccion ?? DBNull.Value);

                    var tvp = cmd.Parameters.AddWithValue("@DetalleVenta", DetalleVenta);
                    tvp.SqlDbType = SqlDbType.Structured;

                    cmd.Parameters.Add("@Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("@Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    resultado = Convert.ToBoolean(cmd.Parameters["@Resultado"].Value);
                    Mensaje = cmd.Parameters["@Mensaje"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                Mensaje = ex.Message;
            }

            return resultado;
        }

        // Listar ventas por cliente (simple): retornamos info básica
        public List<Venta> ListarVentasPorCliente(int idCliente)
        {
            var lista = new List<Venta>();

            try
            {
                using (SqlConnection cn = new SqlConnection(Conexion.cn))
                // La consulta usa la columna real FechaVenta y la mapea al alias FechaRegistro esperado por el mapeo en C#
                using (SqlCommand cmd = new SqlCommand("SELECT IdVenta, IdCliente, TotalProducto, MontoTotal, Contacto, IdCiudad, Telefono, Direccion, FechaVenta AS FechaRegistro, IdTransaccion FROM VENTA WHERE IdCliente = @IdCliente ORDER BY FechaVenta DESC", cn))
                {
                    cmd.Parameters.AddWithValue("@IdCliente", idCliente);
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Venta
                            {
                                IdVenta = Convert.ToInt32(dr["IdVenta"]),
                                IdCliente = Convert.ToInt32(dr["IdCliente"]),
                                TotalProducto = Convert.ToDecimal(dr["TotalProducto"]),
                                MontoTotal = Convert.ToDecimal(dr["MontoTotal"]),
                                Contacto = dr["Contacto"].ToString(),
                                IdCiudad = dr["IdCiudad"].ToString(),
                                Telefono = dr["Telefono"].ToString(),
                                Direccion = dr["Direccion"].ToString(),
                                FechaTexto = Convert.ToDateTime(dr["FechaRegistro"]).ToString("yyyy-MM-dd HH:mm"),
                                IdTransaccion = dr["IdTransaccion"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CD_Venta.ListarVentasPorCliente: " + ex.Message);
            }

            return lista;
        }

        // Obtener detalle por IdVenta
        public List<DetalleVenta> ObtenerDetalleVenta(int idVenta)
        {
            var lista = new List<DetalleVenta>();

            try
            {
                using (SqlConnection cn = new SqlConnection(Conexion.cn))
                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT dv.IdDetalleVenta, dv.IdVenta, dv.IdProducto, dv.Cantidad, dv.Total,
                           p.Nombre, p.Precio, p.RutaImagen, p.NombreImagen
                    FROM DETALLE_VENTA dv
                    INNER JOIN PRODUCTO p ON p.IdProducto = dv.IdProducto
                    WHERE dv.IdVenta = @IdVenta", cn))
                {
                    cmd.Parameters.AddWithValue("@IdVenta", idVenta);
                    cn.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var producto = new Producto
                            {
                                IdProducto = Convert.ToInt32(dr["IdProducto"]),
                                Nombre = dr["Nombre"].ToString(),
                                Precio = Convert.ToDecimal(dr["Precio"]),
                                RutaImagen = dr["RutaImagen"]?.ToString(),
                                NombreImagen = dr["NombreImagen"]?.ToString()
                            };

                            lista.Add(new DetalleVenta
                            {
                                IdDetalleVenta = Convert.ToInt32(dr["IdDetalleVenta"]),
                                IdVenta = Convert.ToInt32(dr["IdVenta"]),
                                oProducto = producto,
                                Cantidad = Convert.ToInt32(dr["Cantidad"]),
                                Total = Convert.ToDecimal(dr["Total"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CD_Venta.ObtenerDetalleVenta: " + ex.Message);
            }

            return lista;
        }
    }
}
