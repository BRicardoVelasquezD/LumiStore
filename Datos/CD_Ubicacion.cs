using Entidad;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class CD_Ubicacion
    {
        public List<Departamento> ObtenerDepartamento()
        {
            List<Departamento> lista = new List<Departamento>();
            try
            {
                using (SqlConnection oconexion = Conexion.CrearConexion())
                {
                    SqlCommand cmd = new SqlCommand("SELECT IdDepartamento, Descripcion FROM DEPARTAMENTO ORDER BY Descripcion", oconexion);
                    oconexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Departamento()
                            {
                                IdDepartamento = dr["IdDepartamento"] != DBNull.Value ? dr["IdDepartamento"].ToString() : "",
                                Descripcion = dr["Descripcion"] != DBNull.Value ? dr["Descripcion"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerDepartamento: " + ex.Message);
                lista = new List<Departamento>();
            }
            return lista;
        }

        public List<Municipio> ObtenerMunicipio(string iddepartamento)
        {
            List<Municipio> lista = new List<Municipio>();
            try
            {
                using (SqlConnection oconexion = Conexion.CrearConexion())
                {
                    SqlCommand cmd = new SqlCommand("SELECT IdMunicipio, Descripcion FROM MUNICIPIO WHERE IdDepartamento = @iddep ORDER BY Descripcion", oconexion);
                    cmd.Parameters.AddWithValue("@iddep", iddepartamento);
                    cmd.CommandType = CommandType.Text;
                    oconexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Municipio()
                            {
                                IdMunicipio = dr["IdMunicipio"] != DBNull.Value ? dr["IdMunicipio"].ToString() : "",
                                Descripcion = dr["Descripcion"] != DBNull.Value ? dr["Descripcion"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerMunicipio: " + ex.Message);
                lista = new List<Municipio>();
            }
            return lista;
        }

        public List<Ciudad> ObtenerCiudad(string idmunicipio, string iddepartamento)
        {
            List<Ciudad> lista = new List<Ciudad>();
            try
            {
                using (SqlConnection oconexion = Conexion.CrearConexion())
                {
                    SqlCommand cmd = new SqlCommand("SELECT IdCiudad, Descripcion FROM CIUDAD WHERE IdMunicipio = @idm AND IdDepartamento = @idd ORDER BY Descripcion", oconexion);
                    cmd.Parameters.AddWithValue("@idm", idmunicipio);
                    cmd.Parameters.AddWithValue("@idd", iddepartamento);
                    oconexion.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            lista.Add(new Ciudad()
                            {
                                IdCiudad = dr["IdCiudad"] != DBNull.Value ? dr["IdCiudad"].ToString() : "",
                                Descripcion = dr["Descripcion"] != DBNull.Value ? dr["Descripcion"].ToString() : ""
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error en ObtenerCiudad: " + ex.Message);
                lista = new List<Ciudad>();
            }
            return lista;
        }
    }
}