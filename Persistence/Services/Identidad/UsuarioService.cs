using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Identidad;
using Application.Exceptions;
using Application.Services.Identidad;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Persistence.Connections;
using Persistence.Constants;
using Persistence.Identity;

namespace Persistence.Services.Identidad
{
    /// <summary>
    /// Las escrituras pasan por <see cref="UserManager{TUser}"/>, que es quien
    /// aplica la política de contraseñas, el hash, la normalización del correo y
    /// el bloqueo por intentos fallidos. Las lecturas siguen por Dapper.
    /// </summary>
    public class UsuarioService : IUsuarioService
    {
        private static readonly string ColumnasUsuario = $@"
            SELECT  Id,
                    Nombres,
                    Apellidos,
                    Email AS Correo,
                    TipoUsuario,
                    Cedula,
                    PhoneNumber AS Telefono,
                    EmailConfirmed AS CorreoConfirmado,
                    FechaRegistro
            FROM    {Tablas.Usuarios}";

        private readonly UserManager<Usuario> _userManager;
        private readonly ISqlConnectionFactory _conexiones;
        private readonly TimeProvider _reloj;

        public UsuarioService(UserManager<Usuario> userManager, ISqlConnectionFactory conexiones, TimeProvider reloj)
        {
            _userManager = userManager;
            _conexiones = conexiones;
            _reloj = reloj;
        }

        public async Task<UsuarioDto> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken cancellationToken = default)
        {
            var correo = dto.Correo.Trim();

            var usuario = new Usuario
            {
                UserName = correo,
                Email = correo,
                PhoneNumber = Normalizar(dto.Telefono),
                Nombres = dto.Nombres.Trim(),
                Apellidos = dto.Apellidos.Trim(),
                TipoUsuario = dto.TipoUsuario,
                Cedula = Normalizar(dto.Cedula),
                FechaRegistro = _reloj.GetUtcNow().UtcDateTime
            };

            GarantizarExito(await _userManager.CreateAsync(usuario, dto.Contrasena));

            return Proyectar(usuario);
        }

        public async Task<UsuarioDto?> AutenticarAsync(CredencialesDto dto, CancellationToken cancellationToken = default)
        {
            var usuario = await _userManager.FindByEmailAsync(dto.Correo.Trim());
            if (usuario is null)
            {
                return null;
            }

            if (await _userManager.IsLockedOutAsync(usuario))
            {
                throw new BusinessRuleException(
                    "La cuenta está bloqueada temporalmente por intentos fallidos. Intente más tarde.");
            }

            if (!await _userManager.CheckPasswordAsync(usuario, dto.Contrasena))
            {
                // Identity cuenta el fallo y bloquea la cuenta al llegar al tope.
                await _userManager.AccessFailedAsync(usuario);
                return null;
            }

            await _userManager.ResetAccessFailedCountAsync(usuario);

            return Proyectar(usuario);
        }

        public async Task<UsuarioDto> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            using var conexion = _conexiones.CrearConexion();
            var usuario = await conexion.QuerySingleOrDefaultAsync<UsuarioDto>(
                new CommandDefinition(
                    $"{ColumnasUsuario} WHERE Id = @id AND IsDeleted = 0",
                    new { id },
                    cancellationToken: cancellationToken));

            return usuario ?? throw new NotFoundException("el usuario", id);
        }

        public async Task<UsuarioDto?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default)
        {
            // Se compara contra NormalizedEmail porque es la columna indexada que
            // mantiene Identity.
            var normalizado = _userManager.NormalizeEmail(correo.Trim());

            using var conexion = _conexiones.CrearConexion();
            return await conexion.QuerySingleOrDefaultAsync<UsuarioDto>(
                new CommandDefinition(
                    $"{ColumnasUsuario} WHERE NormalizedEmail = @normalizado AND IsDeleted = 0",
                    new { normalizado },
                    cancellationToken: cancellationToken));
        }

        public async Task<UsuarioDto> ActualizarPerfilAsync(Guid id, ActualizarPerfilDto dto, CancellationToken cancellationToken = default)
        {
            var usuario = await ObtenerParaEscrituraAsync(id);

            usuario.Nombres = dto.Nombres.Trim();
            usuario.Apellidos = dto.Apellidos.Trim();
            usuario.Cedula = Normalizar(dto.Cedula);
            usuario.PhoneNumber = Normalizar(dto.Telefono);

            GarantizarExito(await _userManager.UpdateAsync(usuario));

            return Proyectar(usuario);
        }

        public async Task CambiarContrasenaAsync(Guid id, string contrasenaActual, string contrasenaNueva, CancellationToken cancellationToken = default)
        {
            var usuario = await ObtenerParaEscrituraAsync(id);

            GarantizarExito(await _userManager.ChangePasswordAsync(usuario, contrasenaActual, contrasenaNueva));
        }

        public async Task<string> GenerarTokenConfirmacionCorreoAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var usuario = await ObtenerParaEscrituraAsync(id);

            return await _userManager.GenerateEmailConfirmationTokenAsync(usuario);
        }

        public async Task ConfirmarCorreoAsync(Guid id, string token, CancellationToken cancellationToken = default)
        {
            var usuario = await ObtenerParaEscrituraAsync(id);

            GarantizarExito(await _userManager.ConfirmEmailAsync(usuario, token));
        }

        public async Task<string?> GenerarTokenRestablecimientoAsync(string correo, CancellationToken cancellationToken = default)
        {
            var usuario = await _userManager.FindByEmailAsync(correo.Trim());
            if (usuario is null)
            {
                return null;
            }

            return await _userManager.GeneratePasswordResetTokenAsync(usuario);
        }

        public async Task RestablecerContrasenaAsync(string correo, string token, string contrasenaNueva, CancellationToken cancellationToken = default)
        {
            var usuario = await _userManager.FindByEmailAsync(correo.Trim())
                ?? throw new NotFoundException($"No existe un usuario con el correo {correo.Trim()}.");

            GarantizarExito(await _userManager.ResetPasswordAsync(usuario, token, contrasenaNueva));
        }

        public async Task EliminarAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var usuario = await ObtenerParaEscrituraAsync(id);

            // Borrado lógico: DeleteAsync borraría la fila y dejaría solicitudes
            // y calificaciones huérfanas.
            usuario.IsDeleted = true;
            GarantizarExito(await _userManager.UpdateAsync(usuario));
        }

        private async Task<Usuario> ObtenerParaEscrituraAsync(Guid id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());
            return usuario ?? throw new NotFoundException("el usuario", id);
        }

        /// <summary>
        /// Traduce el resultado de Identity a la excepción de negocio de la
        /// aplicación, conservando los motivos que dio.
        /// </summary>
        private static void GarantizarExito(IdentityResult resultado)
        {
            if (resultado.Succeeded)
            {
                return;
            }

            var motivos = string.Join(" ", resultado.Errors.Select(e => e.Description));
            throw new BusinessRuleException(motivos);
        }

        private static string? Normalizar(string? valor)
            => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        private static UsuarioDto Proyectar(Usuario usuario) => new(
            usuario.Id,
            usuario.Nombres,
            usuario.Apellidos,
            usuario.Email ?? string.Empty,
            usuario.TipoUsuario,
            usuario.Cedula,
            usuario.PhoneNumber,
            usuario.EmailConfirmed,
            usuario.FechaRegistro);
    }
}
