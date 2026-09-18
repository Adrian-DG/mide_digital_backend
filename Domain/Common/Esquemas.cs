namespace Domain.Common
{
    /// <summary>
    /// Esquemas de base de datos, uno por módulo funcional de la plataforma.
    /// Cada configuración de EF debe declarar el esquema de su entidad con estas
    /// constantes en lugar de literales sueltos.
    /// </summary>
    public static class Esquemas
    {
        /// <summary>Usuarios, perfiles de acceso y credenciales.</summary>
        public const string Identidad = "identidad";

        /// <summary>Directorio institucional: dependencias, servicios y sus requisitos.</summary>
        public const string Catalogo = "catalogo";

        /// <summary>Módulo transaccional: solicitudes y documentos adjuntos.</summary>
        public const string Tramites = "tramites";

        /// <summary>Contenido institucional sindicado (DIECOM / J-5).</summary>
        public const string Contenido = "contenido";

        /// <summary>Retroalimentación de los usuarios sobre los servicios.</summary>
        public const string Retroalimentacion = "retroalimentacion";

        /// <summary>Reservado para el módulo de notificaciones push.</summary>
        public const string Notificaciones = "notificaciones";
    }
}
