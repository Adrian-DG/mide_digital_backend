using System.Security.Claims;
using Application.Abstraction;
using Microsoft.IdentityModel.JsonWebTokens;

namespace API.Authentication;

/// <summary>
/// Lee del token quién hace la petición. Es la implementación que espera el
/// interceptor de auditoría de la capa de persistencia, y se registra antes de
/// AddPersistence para que gane sobre la de reserva.
/// </summary>
public class UsuarioActual : IUsuarioActual
{
    private readonly IHttpContextAccessor _accessor;

    public UsuarioActual(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid? UsuarioId
    {
        get
        {
            var principal = _accessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // MapInboundClaims está desactivado, así que "sub" llega tal cual.
            // Se consulta también NameIdentifier por si algún token lo trae mapeado.
            var valor = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                        ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(valor, out var id) ? id : null;
        }
    }
}
