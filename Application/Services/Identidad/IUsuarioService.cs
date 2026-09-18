using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Identidad;

namespace Application.Services.Identidad
{
    public interface IUsuarioService
    {
        Task<UsuarioDto> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken cancellationToken = default);

        /// <summary>Valida las credenciales. Devuelve null si no coinciden.</summary>
        Task<UsuarioDto?> AutenticarAsync(CredencialesDto dto, CancellationToken cancellationToken = default);

        Task<UsuarioDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

        Task<UsuarioDto?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default);

        Task<UsuarioDto> ActualizarPerfilAsync(int id, ActualizarPerfilDto dto, CancellationToken cancellationToken = default);

        Task CambiarContrasenaAsync(int id, string contrasenaActual, string contrasenaNueva, CancellationToken cancellationToken = default);

        Task ConfirmarCorreoAsync(int id, CancellationToken cancellationToken = default);

        Task EliminarAsync(int id, CancellationToken cancellationToken = default);
    }
}
