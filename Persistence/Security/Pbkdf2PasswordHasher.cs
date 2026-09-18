using System;
using System.Security.Cryptography;
using Application.Abstraction;

namespace Persistence.Security
{
    /// <summary>
    /// Deriva la contrasena con PBKDF2-SHA256 y la almacena como
    /// {iteraciones}.{salt}.{hash} en Base64, para poder subir el costo
    /// mas adelante sin invalidar los hashes existentes.
    /// </summary>
    public class Pbkdf2PasswordHasher : IPasswordHasher
    {
        private const int Iteraciones = 210_000;
        private const int TamanoSalt = 16;
        private const int TamanoHash = 32;

        public string Hash(string contrasena)
        {
            if (string.IsNullOrWhiteSpace(contrasena))
            {
                throw new ArgumentException("La contrasena no puede estar vacia.", nameof(contrasena));
            }

            var salt = RandomNumberGenerator.GetBytes(TamanoSalt);
            var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, salt, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);

            return $"{Iteraciones}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool Verificar(string contrasena, string hashAlmacenado)
        {
            if (string.IsNullOrWhiteSpace(contrasena) || string.IsNullOrWhiteSpace(hashAlmacenado))
            {
                return false;
            }

            var partes = hashAlmacenado.Split('.', 3);
            if (partes.Length != 3 || !int.TryParse(partes[0], out var iteraciones))
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(partes[1]);
                var esperado = Convert.FromBase64String(partes[2]);
                var calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, salt, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

                return CryptographicOperations.FixedTimeEquals(calculado, esperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
