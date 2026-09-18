using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Persistence.Connections
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("La cadena de conexion no puede estar vacia.", nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        public IDbConnection CrearConexion()
        {
            var conexion = new SqlConnection(_connectionString);
            conexion.Open();
            return conexion;
        }
    }
}
