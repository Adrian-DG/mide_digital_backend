using System.Data;

namespace Persistence.Connections
{
    /// <summary>
    /// Abre conexiones para el modelo de lectura (Dapper). Separada del DbContext
    /// a proposito: las consultas no participan del seguimiento de cambios de EF.
    /// </summary>
    public interface ISqlConnectionFactory
    {
        IDbConnection CrearConexion();
    }
}
