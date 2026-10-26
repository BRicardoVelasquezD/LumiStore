using System.Configuration;

using System.Data.Common;
using System.Data.SqlClient;
using Azure.Core;
using Azure.Identity;

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

        public static SqlConnection CrearConexion()
        {
            var builder = new DbConnectionStringBuilder { ConnectionString = cn };
            string dataSource = GetValue(builder, "Data Source") ?? GetValue(builder, "Server") ?? string.Empty;
            bool useManagedIdentity = dataSource.IndexOf(".database.windows.net", System.StringComparison.OrdinalIgnoreCase) >= 0;

            if (useManagedIdentity)
            {
                if (builder.ContainsKey("Authentication"))
                    builder.Remove("Authentication");

                var connection = new SqlConnection(builder.ConnectionString);
                var credential = new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned);
                var token = credential.GetToken(new TokenRequestContext(new[] { "https://database.windows.net/.default" }));
                connection.AccessToken = token.Token;
                return connection;
            }

            return new SqlConnection(builder.ConnectionString);
        }

        private static string GetValue(DbConnectionStringBuilder builder, string key)
        {
            return builder.TryGetValue(key, out object value) ? value?.ToString() : null;
        }
    }
}

