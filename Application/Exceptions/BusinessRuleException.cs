using System;

namespace Application.Exceptions
{
    /// <summary>
    /// Una regla de negocio impide completar la operación (por ejemplo, enviar
    /// una solicitud sin los requisitos obligatorios). Se traduce a un 400/409.
    /// </summary>
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string mensaje) : base(mensaje)
        {
        }
    }
}
