using Datos;
using Entidad;
using System;
using System.Collections.Generic;
using System.IO;

namespace Negocio
{
    public class CN_Producto
    {
        private CD_Producto objDato = new CD_Producto();

        public List<Producto> Listar() => objDato.Listar();

        public int Registrar(Producto obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(obj.Nombre)) Mensaje = "El nombre es obligatorio";
            if (obj.Precio <= 0) Mensaje = "Debe ingresar un precio válido";

            if (string.IsNullOrEmpty(Mensaje)) return objDato.Registrar(obj, out Mensaje);
            return 0;
        }

        public bool Editar(Producto obj, out string Mensaje)
        {
            Mensaje = string.Empty;
            if (string.IsNullOrEmpty(obj.Nombre)) Mensaje = "El nombre es obligatorio";
            if (obj.Precio <= 0) Mensaje = "El precio debe ser mayor a 0";

            if (string.IsNullOrEmpty(Mensaje)) return objDato.Editar(obj, out Mensaje);
            return false;
        }

        public bool ActualizarRutaImagen(Producto obj, out string Mensaje)
        {
            return objDato.ActualizarRutaImagen(obj, out Mensaje);
        }

        public bool Eliminar(int id, out string Mensaje) => objDato.Eliminar(id, out Mensaje);

        public string ConvertirBase64(string ruta, out bool conversion)
        {
            string textoBase64 = string.Empty;
            conversion = true;
            try
            {
                if (File.Exists(ruta))
                {
                    byte[] bytes = File.ReadAllBytes(ruta);
                    textoBase64 = Convert.ToBase64String(bytes);
                }
                else
                {
                    conversion = false;
                }
            }
            catch
            {
                conversion = false;
            }
            return textoBase64;
        }
    }
}