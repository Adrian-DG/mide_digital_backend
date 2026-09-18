using Domain.Common;

namespace Persistence.Constants
{
    /// <summary>
    /// Nombres de tabla calificados con su esquema. Las consultas de Dapper los
    /// usan en lugar de literales, para que un cambio de esquema o de nombre no
    /// obligue a revisar cada handler.
    /// </summary>
    public static class Tablas
    {
        public const string Usuarios = Esquemas.Identidad + ".Usuarios";

        public const string Dependencias = Esquemas.Catalogo + ".Dependencias";
        public const string CategoriasServicio = Esquemas.Catalogo + ".CategoriasServicio";
        public const string Servicios = Esquemas.Catalogo + ".Servicios";
        public const string Requisitos = Esquemas.Catalogo + ".Requisitos";

        public const string Solicitudes = Esquemas.Tramites + ".Solicitudes";
        public const string DocumentosSolicitud = Esquemas.Tramites + ".DocumentosSolicitud";

        public const string Noticias = Esquemas.Contenido + ".Noticias";

        public const string CalificacionesServicio = Esquemas.Retroalimentacion + ".CalificacionesServicio";
    }
}
