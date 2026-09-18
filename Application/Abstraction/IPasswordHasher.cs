namespace Application.Abstraction
{
    /// <summary>
    /// Derivación y verificación de contraseñas. El dominio solo conoce el hash.
    /// </summary>
    public interface IPasswordHasher
    {
        string Hash(string contrasena);

        bool Verificar(string contrasena, string hashAlmacenado);
    }
}
