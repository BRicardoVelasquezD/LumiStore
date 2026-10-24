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
            Mensaje = string.Empty;

            try
            {
                using (SqlConnection oconexion = new SqlConnection(Conexion.cn))
                {
                    oconexion.Open();
                    using (SqlTransaction transaccion = oconexion.BeginTransaction(IsolationLevel.Serializable))
                    {
                        decimal subtotal = 0m;
                        int cantidadTotal = 0;
                        var lineas = new List<Tuple<int, int, decimal>>();

                        foreach (DataRow fila in DetalleVenta.Rows)
                        {
                            int idProducto = Convert.ToInt32(fila["IdProducto"]);
                            int cantidad = Convert.ToInt32(fila["Cantidad"]);
                            if (idProducto <= 0 || cantidad <= 0)
                                throw new InvalidOperationException("El detalle de la venta no es válido.");

                            decimal precio;
                            int stock;
                            bool activo;
                            using (SqlCommand cmd = new SqlCommand("SELECT Precio, Stock, Activo FROM PRODUCTO WITH (UPDLOCK, HOLDLOCK) WHERE IdProducto = @IdProducto", oconexion, transaccion))
                            {
                                cmd.Parameters.Add("@IdProducto", SqlDbType.Int).Value = idProducto;
                                using (SqlDataReader dr = cmd.ExecuteReader())
                                {
                                    if (!dr.Read())
                                        throw new InvalidOperationException("Un producto de la venta ya no existe.");
                                    precio = Convert.ToDecimal(dr["Precio"]);
                                    stock = Convert.ToInt32(dr["Stock"]);
                                    activo = dr["Activo"] != DBNull.Value && Convert.ToBoolean(dr["Activo"]);
                                }
                            }

                            if (!activo || stock < cantidad)
                                throw new InvalidOperationException("El inventario cambió; actualiza el carrito e intenta nuevamente.");

                            decimal totalLinea = decimal.Round(precio * cantidad, 2, MidpointRounding.AwayFromZero);
                            decimal totalRecibido = Convert.ToDecimal(fila["Total"]);
                            if (totalLinea != totalRecibido)
                                throw new InvalidOperationException("El precio cambió; vuelve a iniciar el pago.");

                            using (SqlCommand cmd = new SqlCommand("UPDATE PRODUCTO SET Stock = Stock - @Cantidad WHERE IdProducto = @IdProducto AND Stock >= @Cantidad AND Activo = 1", oconexion, transaccion))
                            {
                                cmd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = cantidad;
                                cmd.Parameters.Add("@IdProducto", SqlDbType.Int).Value = idProducto;
                                if (cmd.ExecuteNonQuery() != 1)
                                    throw new InvalidOperationException("No se pudo reservar el inventario.");
                            }

                            subtotal += totalLinea;
                            cantidadTotal += cantidad;
                            lineas.Add(Tuple.Create(idProducto, cantidad, totalLinea));
                        }

                        decimal envio = subtotal == 0m || subtotal >= 500000m ? 0m : 15000m;
                        decimal totalEsperado = decimal.Round(subtotal + decimal.Round(subtotal * 0.19m, 2, MidpointRounding.AwayFromZero) + envio, 2, MidpointRounding.AwayFromZero);
                        if (lineas.Count == 0 || Convert.ToDecimal(obj.MontoTotal) != totalEsperado)
                            throw new InvalidOperationException("El total del pago no coincide con la venta.");

                        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(1) FROM VENTA WHERE IdTransaccion = @IdTransaccion", oconexion, transaccion))
                        {
                            cmd.Parameters.Add("@IdTransaccion", SqlDbType.VarChar, 50).Value = obj.IdTransaccion;
                            if (Convert.ToInt32(cmd.ExecuteScalar()) > 0)
                                throw new InvalidOperationException("Esta transacción ya fue registrada.");
                        }

                        int idVenta;
                        using (SqlCommand cmd = new SqlCommand(@"INSERT INTO VENTA (IdCliente, TotalProducto, MontoTotal, Contacto, IdCiudad, Telefono, Direccion, IdTransaccion, FechaVenta)
VALUES (@IdCliente, @TotalProducto, @MontoTotal, @Contacto, @IdCiudad, @Telefono, @Direccion, @IdTransaccion, GETDATE());
SELECT CAST(SCOPE_IDENTITY() AS int);", oconexion, transaccion))
                        {
                            cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = obj.IdCliente;
                            cmd.Parameters.Add("@TotalProducto", SqlDbType.Int).Value = cantidadTotal;
                            cmd.Parameters.Add("@MontoTotal", SqlDbType.Decimal).Value = totalEsperado;
                            cmd.Parameters["@MontoTotal"].Precision = 18;
                            cmd.Parameters["@MontoTotal"].Scale = 2;
                            cmd.Parameters.Add("@Contacto", SqlDbType.VarChar, 50).Value = (object)obj.Contacto ?? DBNull.Value;
                            cmd.Parameters.Add("@IdCiudad", SqlDbType.VarChar, 10).Value = (object)obj.IdCiudad ?? DBNull.Value;
                            cmd.Parameters.Add("@Telefono", SqlDbType.VarChar, 50).Value = (object)obj.Telefono ?? DBNull.Value;
                            cmd.Parameters.Add("@Direccion", SqlDbType.VarChar, 500).Value = (object)obj.Direccion ?? DBNull.Value;
                            cmd.Parameters.Add("@IdTransaccion", SqlDbType.VarChar, 50).Value = obj.IdTransaccion;
                            idVenta = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (var linea in lineas)
                        {
                            using (SqlCommand cmd = new SqlCommand("INSERT INTO DETALLE_VENTA (IdVenta, IdProducto, Cantidad, Total) VALUES (@IdVenta, @IdProducto, @Cantidad, @Total)", oconexion, transaccion))
                            {
                                cmd.Parameters.Add("@IdVenta", SqlDbType.Int).Value = idVenta;
                                cmd.Parameters.Add("@IdProducto", SqlDbType.Int).Value = linea.Item1;
                                cmd.Parameters.Add("@Cantidad", SqlDbType.Int).Value = linea.Item2;
                                cmd.Parameters.Add("@Total", SqlDbType.Decimal).Value = linea.Item3;
                                cmd.Parameters["@Total"].Precision = 18;
                                cmd.Parameters["@Total"].Scale = 2;
                                cmd.ExecuteNonQuery();
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand("DELETE FROM CARRITO WHERE IdCliente = @IdCliente", oconexion, transaccion))
                        {
                            cmd.Parameters.Add("@IdCliente", SqlDbType.Int).Value = obj.IdCliente;
                            cmd.ExecuteNonQuery();
                        }

                        transaccion.Commit();
                    }

                    Mensaje = "Venta registrada.";
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CD_Venta.Registrar: " + ex.Message);
                Mensaje = ex.Message;
                return false;
            }
        }

        // Organiza sección
        public List<Venta> ListarVentasPorCliente(int idCliente)
        {
            var lista = new List<Venta>();

            try
            {
                using (SqlConnection cn = new SqlConnection(Conexion.cn))
                // Organiza sección
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

        // Organiza sección
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
