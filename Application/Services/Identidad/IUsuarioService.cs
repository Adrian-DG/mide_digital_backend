using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Identidad;

namespace Application.Services.Identidad
{
    public interface IUsuarioService
    {
        /// <summary>
        /// Registra al usuario. Si Identity rechaza el correo o la contraseña
        /// (duplicado, política de complejidad), lanza BusinessRuleException con
        /// los motivos.
        /// </summary>
        Task<UsuarioDto> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Valida las credenciales contra Identity y lleva la cuenta de intentos
        /// fallidos. Devuelve null si no coinciden; lanza BusinessRuleException si
        /// la cuenta está bloqueada por intentos fallidos.
        /// </summary>
        Task<UsuarioDto?> AutenticarAsync(CredencialesDto dto, CancellationToken cancellationToken = default);

        Task<UsuarioDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

        Task<UsuarioDto?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default);

        Task<UsuarioDto> ActualizarPerfilAsync(int id, ActualizarPerfilDto dto, CancellationToken cancellationToken = default);

        Task CambiarContrasenaAsync(int id, string contrasenaActual, string contrasenaNueva, CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera el token de confirmación de correo. Quien lo envía al usuario es
        /// la capa que tenga el servicio de correo, no esta.
        /// </summary>
        Task<string> GenerarTokenConfirmacionCorreoAsync(int id, CancellationToken cancellationToken = default);

        Task ConfirmarCorreoAsync(int id, string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Genera el token de restablecimiento. Devuelve null si el correo no
        /// existe, para no revelar qué cuentas están registradas.
        /// </summary>
        Task<string?> GenerarTokenRestablecimientoAsync(string correo, CancellationToken cancellationToken = default);

        Task RestablecerContrasenaAsync(string correo, string token, string contrasenaNueva, CancellationToken cancellationToken = default);

        Task EliminarAsync(int id, CancellationToken cancellationToken = default);
    }
}
