using System.Security.Claims;
using System.Text;
using Application.DTOs.Identidad;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace API.Authentication;

/// <summary>
/// Emite el JWT de acceso. Vive en esta capa porque la firma y la audiencia son
/// configuración del host, no de la persistencia.
/// </summary>
public class GeneradorTokensJwt : IGeneradorTokens
{
    /// <summary>Clasificación funcional del perfil, para filtrar la oferta de servicios.</summary>
    public const string ClaimTipoUsuario = "tipo_usuario";

    /// <summary>Si el correo está confirmado; algunos trámites lo van a exigir.</summary>
    public const string ClaimCorreoConfirmado = "correo_confirmado";

    private readonly JwtOptions _opciones;
    private readonly TimeProvider _reloj;

    public GeneradorTokensJwt(IOptions<JwtOptions> opciones, TimeProvider reloj)
    {
        _opciones = opciones.Value;
        _reloj = reloj;
    }

    public TokenEmitido Emitir(UsuarioDto usuario)
    {
        var ahora = _reloj.GetUtcNow().UtcDateTime;
        var expiracion = ahora.AddMinutes(_opciones.MinutosDeVigencia);

        var claves = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.ClaveDeFirma));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _opciones.Issuer,
            Audience = _opciones.Audience,
            IssuedAt = ahora,
            NotBefore = ahora,
            Expires = expiracion,
            SigningCredentials = new SigningCredentials(claves, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = usuario.Id.ToString(),
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                [JwtRegisteredClaimNames.Email] = usuario.Correo,
                [ClaimTypes.Name] = usuario.NombreCompleto,
                [ClaimTipoUsuario] = usuario.TipoUsuario.ToString(),
                [ClaimCorreoConfirmado] = usuario.CorreoConfirmado
            }
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return new TokenEmitido(token, expiracion);
    }
}
