using System;
namespace Application.Abstraction
{
    /// <summary>
    /// Identifica a quién se le atribuye la operación en curso. La implementación
    /// real la aporta la capa que conoce la petición (Presentation, leyendo el
    /// token); la persistencia solo la consume para sellar la auditoría.
    /// </summary>
    public interface IUsuarioActual
    {
        /// <summary>
        /// Identificador del usuario autenticado, o null en procesos sin usuario
        /// (tareas programadas, migraciones, semillas).
        /// </summary>
        Guid? UsuarioId { get; }
    }
}
