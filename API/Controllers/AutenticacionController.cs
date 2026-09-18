using System.Security.Claims;
using API.Authentication;
using API.Contracts.Autenticacion;
using Application.DTOs.Identidad;
using Application.Services.Identidad;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace API.Controllers;

/// <summary>
/// Registro, inicio de sesión y gestión de la propia cuenta.
/// </summary>
/// <remarks>
/// El controlador exige token por defecto: los endpoints que el ciudadano usa
/// antes de tener una sesión se marcan con <see cref="AllowAnonymousAttribute"/>
/// uno por uno, para que agregar un endpoint nuevo no lo deje abierto por
/// descuido.
/// </remarks>
[ApiController]
[Route("api/autenticacion")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Produces("application/json")]
public class AutenticacionController : ControllerBase
{
    private readonly IUsuarioService _usuarios;
    private readonly IGeneradorTokens _tokens;
    private readonly IWebHostEnvironment _entorno;
    private readonly ILogger<AutenticacionController> _logger;

    public AutenticacionController(
        IUsuarioService usuarios,
        IGeneradorTokens tokens,
        IWebHostEnvironment entorno,
        ILogger<AutenticacionController> logger)
    {
        _usuarios = usuarios;
        _tokens = tokens;
        _entorno = entorno;
        _logger = logger;
    }

    /// <summary>Registra un usuario nuevo y devuelve su perfil.</summary>
    [HttpPost("registro")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UsuarioDto>> Registrar(
        RegistroRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.RegistrarAsync(
            new RegistrarUsuarioDto
            {
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                Correo = request.Correo,
                Contrasena = request.Contrasena,
                TipoUsuario = request.TipoUsuario,
                Cedula = request.Cedula,
                Telefono = request.Telefono
            },
            cancellationToken);

        // No se emite token aquí a propósito: el registro no inicia sesión, el
        // cliente llama a /login. Así el flujo es el mismo con y sin correo
        // confirmado obligatorio.
        return CreatedAtAction(nameof(ObtenerPerfil), new { }, usuario);
    }

    /// <summary>Valida credenciales y emite el token de acceso.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SesionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<ActionResult<SesionResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.AutenticarAsync(
            new CredencialesDto { Correo = request.Correo, Contrasena = request.Contrasena },
            cancellationToken);

        if (usuario is null)
        {
            // Mismo mensaje para correo inexistente y contraseña incorrecta: no
            // hay que revelar qué cuentas existen.
            return Problem(
                title: "Credenciales inválidas",
                detail: "El correo o la contraseña no son correctos.",
                statusCode: StatusCodes.Status401Unauthorized);
        }

        var token = _tokens.Emitir(usuario);

        return Ok(new SesionResponse(
            token.Token,
            JwtBearerDefaults.AuthenticationScheme,
            token.ExpiraEn,
            usuario));
    }

    /// <summary>Perfil del usuario dueño del token.</summary>
    [HttpGet("perfil")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioDto>> ObtenerPerfil(CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.ObtenerPorIdAsync(UsuarioIdDelToken(), cancellationToken);
        return Ok(usuario);
    }

    /// <summary>Actualiza los datos propios del perfil.</summary>
    [HttpPut("perfil")]
    [ProducesResponseType(typeof(UsuarioDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UsuarioDto>> ActualizarPerfil(
        ActualizarPerfilRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await _usuarios.ActualizarPerfilAsync(
            UsuarioIdDelToken(),
            new ActualizarPerfilDto
            {
                Nombres = request.Nombres,
                Apellidos = request.Apellidos,
                Cedula = request.Cedula,
                Telefono = request.Telefono
            },
            cancellationToken);

        return Ok(usuario);
    }

    /// <summary>Cambia la contraseña del usuario autenticado.</summary>
    [HttpPost("contrasena/cambio")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CambiarContrasena(
        CambiarContrasenaRequest request,
        CancellationToken cancellationToken)
    {
        await _usuarios.CambiarContrasenaAsync(
            UsuarioIdDelToken(),
            request.ContrasenaActual,
            request.ContrasenaNueva,
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Genera el token de confirmación de correo del usuario autenticado.
    /// </summary>
    [HttpPost("correo/confirmacion/solicitud")]
    [ProducesResponseType(typeof(TokenCorreoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TokenCorreoResponse>> SolicitarConfirmacionDeCorreo(
        CancellationToken cancellationToken)
    {
        var usuarioId = UsuarioIdDelToken();
        var token = await _usuarios.GenerarTokenConfirmacionCorreoAsync(usuarioId, cancellationToken);

        return Ok(ResponderConToken(
            token,
            usuarioId,
            "Se generó el enlace de confirmación de correo."));
    }

    /// <summary>Confirma el correo con el token recibido.</summary>
    [HttpPost("correo/confirmacion")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConfirmarCorreo(
        ConfirmarCorreoRequest request,
        CancellationToken cancellationToken)
    {
        await _usuarios.ConfirmarCorreoAsync(request.UsuarioId, request.Token, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Inicia la recuperación de contraseña. Responde igual exista o no la
    /// cuenta, para no revelar qué correos están registrados.
    /// </summary>
    [HttpPost("contrasena/recuperacion")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenCorreoResponse), StatusCodes.Status202Accepted)]
    public async Task<ActionResult<TokenCorreoResponse>> RecuperarContrasena(
        RecuperarContrasenaRequest request,
        CancellationToken cancellationToken)
    {
        var token = await _usuarios.GenerarTokenRestablecimientoAsync(request.Correo, cancellationToken);

        const string mensaje = "Si el correo está registrado, recibirá las instrucciones para restablecer la contraseña.";

        if (token is null)
        {
            return Accepted(new TokenCorreoResponse(mensaje, null));
        }

        return Accepted(ResponderConToken(token, request.Correo, mensaje));
    }

    /// <summary>Restablece la contraseña con el token de recuperación.</summary>
    [HttpPost("contrasena/restablecimiento")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RestablecerContrasena(
        RestablecerContrasenaRequest request,
        CancellationToken cancellationToken)
    {
        await _usuarios.RestablecerContrasenaAsync(
            request.Correo,
            request.Token,
            request.ContrasenaNueva,
            cancellationToken);

        return NoContent();
    }

    /// <summary>Da de baja la propia cuenta (borrado lógico).</summary>
    [HttpDelete("cuenta")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> EliminarCuenta(CancellationToken cancellationToken)
    {
        await _usuarios.EliminarAsync(UsuarioIdDelToken(), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Identificador del usuario tomado del claim <c>sub</c>. Si falta o no es
    /// un Guid, el token es válido pero inservible, así que es un fallo del
    /// emisor y no del cliente.
    /// </summary>
    private Guid UsuarioIdDelToken()
    {
        var valor = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                    ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(valor, out var id)
            ? id
            : throw new InvalidOperationException("El token no contiene un identificador de usuario válido.");
    }

    /// <summary>
    /// Devuelve el token de correo solo en desarrollo. En cualquier otro entorno
    /// se registra que hay un envío pendiente y el cliente recibe null: mientras
    /// no exista el servicio de correo, este es el punto que hay que conectar.
    /// </summary>
    private TokenCorreoResponse ResponderConToken(string token, object destinatario, string mensaje)
    {
        if (_entorno.IsDevelopment())
        {
            return new TokenCorreoResponse(mensaje, token);
        }

        _logger.LogWarning(
            "Token de correo generado para {Destinatario} pero no hay servicio de correo configurado.",
            destinatario);

        return new TokenCorreoResponse(mensaje, null);
    }
}
