using System;
using Application.Abstraction;

namespace Persistence.Identity
{
    /// <summary>
    /// Implementación de reserva para procesos sin petición HTTP (semillas,
    /// migraciones, tareas programadas). Se registra con TryAdd, así que si
    /// Presentation aporta la suya basada en el token, esa gana.
    /// </summary>
    public class UsuarioActualNoIdentificado : IUsuarioActual
    {
        public Guid? UsuarioId => null;
    }
}
