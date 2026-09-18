namespace Application.Exceptions
{
    /// <summary>
    /// La cuenta está bloqueada por intentos fallidos. Se distingue del resto de
    /// reglas de negocio porque la capa de presentación la traduce a un 423.
    /// </summary>
    public class CuentaBloqueadaException : BusinessRuleException
    {
        public CuentaBloqueadaException(string mensaje) : base(mensaje)
        {
        }
    }
}
