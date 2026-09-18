using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace API.Contracts.Autenticacion;

/// <summary>
/// Contratos de entrada de los endpoints de autenticación. Aquí vive la
/// validación de formato (correo válido, largos, campos obligatorios); las
/// reglas de negocio y la política de contraseñas las aplican Identity y los
/// servicios.
/// </summary>
public record RegistroRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombres { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string Apellidos { get; init; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Correo { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 10)]
    public string Contrasena { get; init; } = string.Empty;

    [Required, EnumDataType(typeof(TipoUsuario))]
    public TipoUsuario TipoUsuario { get; init; }

    [StringLength(13, MinimumLength = 11)]
    [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "La cédula solo admite dígitos y guiones.")]
    public string? Cedula { get; init; }

    [Phone, StringLength(20)]
    public string? Telefono { get; init; }
}

public record LoginRequest
{
    [Required, EmailAddress]
    public string Correo { get; init; } = string.Empty;

    [Required]
    public string Contrasena { get; init; } = string.Empty;
}

public record ActualizarPerfilRequest
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Nombres { get; init; } = string.Empty;

    [Required, StringLength(100, MinimumLength = 2)]
    public string Apellidos { get; init; } = string.Empty;

    [StringLength(13, MinimumLength = 11)]
    [RegularExpression(@"^[0-9\-]+$", ErrorMessage = "La cédula solo admite dígitos y guiones.")]
    public string? Cedula { get; init; }

    [Phone, StringLength(20)]
    public string? Telefono { get; init; }
}

public record CambiarContrasenaRequest
{
    [Required]
    public string ContrasenaActual { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 10)]
    public string ContrasenaNueva { get; init; } = string.Empty;
}

public record ConfirmarCorreoRequest
{
    [Required]
    public Guid UsuarioId { get; init; }

    [Required]
    public string Token { get; init; } = string.Empty;
}

public record RecuperarContrasenaRequest
{
    [Required, EmailAddress]
    public string Correo { get; init; } = string.Empty;
}

public record RestablecerContrasenaRequest
{
    [Required, EmailAddress]
    public string Correo { get; init; } = string.Empty;

    [Required]
    public string Token { get; init; } = string.Empty;

    [Required, StringLength(128, MinimumLength = 10)]
    public string ContrasenaNueva { get; init; } = string.Empty;
}
