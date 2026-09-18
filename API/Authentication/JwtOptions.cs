using System.ComponentModel.DataAnnotations;

namespace API.Authentication;

/// <summary>
/// Parámetros de firma y validación del token. Se validan al arrancar, así que
/// una configuración incompleta detiene la aplicación en vez de fallar en la
/// primera petición.
/// </summary>
public class JwtOptions
{
    public const string SeccionConfiguracion = "Jwt";

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Clave simétrica de firma (HS256). Nunca va en appsettings.json de un
    /// repositorio: en desarrollo va en user-secrets y en producción en el
    /// almacén de secretos del entorno.
    /// </summary>
    [Required]
    [MinLength(32, ErrorMessage = "La clave de firma debe tener al menos 32 caracteres para HS256.")]
    public string ClaveDeFirma { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int MinutosDeVigencia { get; set; } = 60;
}
