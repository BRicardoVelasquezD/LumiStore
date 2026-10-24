using System.Configuration;

namespace Datos
{
    public class Conexion
    {
        public static string cn
        {
            get
            {
                var connection = ConfigurationManager.ConnectionStrings["DatabaseConnection"];
                if (connection == null || string.IsNullOrWhiteSpace(connection.ConnectionString))
                    throw new ConfigurationErrorsException("Configure la cadena DatabaseConnection de Azure SQL.");

                return connection.ConnectionString;
            }
        }
    }
}

