using System;

namespace Application.Exceptions
{
    /// <summary>
    /// El recurso solicitado no existe o está marcado como eliminado.
    /// La capa de presentación la traduce a un 404.
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException(string mensaje) : base(mensaje)
        {
        }

        public NotFoundException(string entidad, object id)
            : base($"No se encontró {entidad} con identificador '{id}'.")
        {
        }
    }
}
