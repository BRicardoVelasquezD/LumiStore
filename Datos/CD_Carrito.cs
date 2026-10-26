using Entidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class CD_Carrito
    {
        public List<Carrito> ListarProducto(int idcliente)
        {
            List<Carrito> lista = new List<Carrito>();
            try
            {
                using (SqlConnection oconexion = Conexion.CrearConexion())
                {
                    SqlCommand cmd = new SqlCommand("select * from fn_obtenerCarritoCliente(@idcliente)", oconexion);
                    cmd.Parameters.AddWithValue("@idcliente", idcliente);
                    cmd.CommandType = CommandType.Text;
                    oconexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Carrito()
                            {
                                oProducto = new Producto()
                                {
                                    IdProducto = Convert.ToInt32(dr["IdProducto"]),
                                    Nombre = dr["Nombre"].ToString(),
                                    Precio = Convert.ToDecimal(dr["Precio"]),
                                    RutaImagen = dr["RutaImagen"] != DBNull.Value ? dr["RutaImagen"].ToString() : "",
                                    NombreImagen = dr["NombreImagen"] != DBNull.Value ? dr["NombreImagen"].ToString() : "",
                                    oMarca = new Marca()
                                    {
                                        Descripcion = dr["DesMarca"] != DBNull.Value ? dr["DesMarca"].ToString() : ""
                                    }
                                },
                                Cantidad = Convert.ToInt32(dr["Cantidad"])
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ListarProducto: " + ex.Message);
                lista = new List<Carrito>();
            }
            return lista;
        }

        public bool ExisteCarrito(int idcliente, int idproducto)
        {
            bool resultado = false;
            try
            {
                using (SqlConnection oconexion = Conexion.CrearConexion())
                {
                    SqlCommand cmd = new SqlCommand("sp_ExisteCarrito", oconexion);
                    cmd.Parameters.AddWithValue("IdCliente", idcliente);
                    cmd.Parameters.AddWithValue("IdProducto", idproducto);
                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    resultado = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ExisteCarrito: " + ex.Message);
                resultado = false;
            }
            return resultado;
        }

        public bool OperacionCarrito(int idcliente, int idproducto, bool sumar, out string Mensaje)
        {
            bool resultado = false;
            Mensaje = string.Empty;
            try
            {
                using (SqlConnection oconexion = Conexion.CrearConexion())
                {
                    SqlCommand cmd = new SqlCommand("sp_OperacionCarrito", oconexion);
                    cmd.Parameters.AddWithValue("IdCliente", idcliente);
                    cmd.Parameters.AddWithValue("IdProducto", idproducto);
                    cmd.Parameters.AddWithValue("Sumar", sumar);
                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 500).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();

                    resultado = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
                    Mensaje = cmd.Parameters["Mensaje"].Value != DBNull.Value ? cmd.Parameters["Mensaje"].Value.ToString() : "";
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                Mensaje = ex.Message;
                System.Diagnostics.Debug.WriteLine("Error en OperacionCarrito: " + ex.Message);
            }
            return resultado;
        }

        public int CantidadEnCarrito(int idcliente)
        {
            int resultado = 0;
            try
            {
                using (SqlConnection oconexion = Conexion.CrearConexion())
                {
                    string query = "select isnull(sum(cantidad),0) from carrito where idcliente = @idcliente";
                    SqlCommand cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@idcliente", idcliente);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();
                    resultado = Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en CantidadEnCarrito: " + ex.Message);
                resultado = 0;
            }
            return resultado;
        }

        public bool EliminarCarrito(int idcliente, int idproducto)
        {
            bool resultado = false;
            try
            {
                using (SqlConnection oconexion = Conexion.CrearConexion())
                {
                    SqlCommand cmd = new SqlCommand("sp_EliminarCarrito", oconexion);
                    cmd.Parameters.AddWithValue("IdCliente", idcliente);
                    cmd.Parameters.AddWithValue("IdProducto", idproducto);
                    cmd.Parameters.Add("Resultado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                    cmd.CommandType = CommandType.StoredProcedure;

                    oconexion.Open();
                    cmd.ExecuteNonQuery();
                    resultado = Convert.ToBoolean(cmd.Parameters["Resultado"].Value);
                }
            }
            catch (Exception ex)
            {
                resultado = false;
                System.Diagnostics.Debug.WriteLine("Error en EliminarCarrito: " + ex.Message);
            }
            return resultado;
        }
    }
}