using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence.Context;
using Persistence.Identity;

namespace Persistence.Seeding
{
    /// <summary>
    /// Crea los <see cref="UsuariosDePrueba"/> y, si ya existen, los devuelve a
    /// su estado inicial: contraseña, intentos fallidos, bloqueo, confirmación
    /// de correo y borrado lógico. Solo debe ejecutarse en desarrollo.
    /// </summary>
    /// <remarks>
    /// La creación pasa por <see cref="UserManager{TUser}"/> para que el hash y
    /// la normalización del correo sean los mismos que en un registro real. El
    /// estado se ajusta directamente sobre la entidad porque Identity no expone
    /// una forma de fijar, por ejemplo, el contador de intentos fallidos.
    /// </remarks>
    public class SembradorUsuariosDePrueba
    {
        /// <summary>
        /// Fin de bloqueo de la cuenta "bloqueada": lo bastante lejano para que
        /// el escenario no caduque solo mientras se prueba.
        /// </summary>
        private static readonly DateTimeOffset BloqueoIndefinido = new(2099, 12, 31, 0, 0, 0, TimeSpan.Zero);

        private readonly MideDbContext _contexto;
        private readonly UserManager<Usuario> _userManager;
        private readonly TimeProvider _reloj;
        private readonly ILogger<SembradorUsuariosDePrueba> _logger;

        public SembradorUsuariosDePrueba(
            MideDbContext contexto,
            UserManager<Usuario> userManager,
            TimeProvider reloj,
            ILogger<SembradorUsuariosDePrueba> logger)
        {
            _contexto = contexto;
            _userManager = userManager;
            _reloj = reloj;
            _logger = logger;
        }

        public async Task SembrarAsync(CancellationToken cancellationToken = default)
        {
            var intentosParaBloquear = _userManager.Options.Lockout.MaxFailedAccessAttempts;

            foreach (var perfil in UsuariosDePrueba.Todos)
            {
                var usuario = await BuscarIncluyendoEliminadosAsync(perfil.Correo, cancellationToken)
                              ?? await CrearAsync(perfil);

                RestablecerContrasenaSiCambio(usuario);

                usuario.EmailConfirmed = perfil.CorreoConfirmado;
                usuario.LockoutEnabled = true;
                usuario.IsDeleted = perfil.Estado == EstadoCuentaDePrueba.Eliminada;
                usuario.AccessFailedCount = perfil.Estado == EstadoCuentaDePrueba.CasiBloqueada
                    ? intentosParaBloquear - 1
                    : 0;
                usuario.LockoutEnd = perfil.Estado == EstadoCuentaDePrueba.Bloqueada
                    ? BloqueoIndefinido
                    : null;
            }

            await _contexto.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Usuarios de prueba listos ({Cantidad}). Contraseña común: {Contrasena}",
                UsuariosDePrueba.Todos.Count,
                UsuariosDePrueba.Contrasena);
        }

        /// <summary>
        /// Ignora el filtro de borrado lógico: la cuenta "eliminada" también se
        /// tiene que encontrar para no intentar crearla otra vez.
        /// </summary>
        private Task<Usuario?> BuscarIncluyendoEliminadosAsync(string correo, CancellationToken cancellationToken)
        {
            var normalizado = _userManager.NormalizeEmail(correo);

            return _contexto.Usuarios
                .IgnoreQueryFilters()
                .SingleOrDefaultAsync(u => u.NormalizedEmail == normalizado, cancellationToken);
        }

        private async Task<Usuario> CrearAsync(PerfilDePrueba perfil)
        {
            var usuario = new Usuario
            {
                UserName = perfil.Correo,
                Email = perfil.Correo,
                PhoneNumber = perfil.Telefono,
                Nombres = perfil.Nombres,
                Apellidos = perfil.Apellidos,
                TipoUsuario = perfil.TipoUsuario,
                Cedula = perfil.Cedula,
                FechaRegistro = _reloj.GetUtcNow().UtcDateTime
            };

            var resultado = await _userManager.CreateAsync(usuario, UsuariosDePrueba.Contrasena);
            if (!resultado.Succeeded)
            {
                var motivos = string.Join(" ", resultado.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"No se pudo crear el usuario de prueba {perfil.Correo}: {motivos}");
            }

            return usuario;
        }

        /// <summary>
        /// Si alguien cambió o restableció la contraseña durante una prueba, se
        /// vuelve a la común. Se verifica antes de rehacer el hash porque cada
        /// hash cuesta del orden de cien milisegundos.
        /// </summary>
        private void RestablecerContrasenaSiCambio(Usuario usuario)
        {
            var hasher = _userManager.PasswordHasher;

            var vigente = usuario.PasswordHash is not null
                          && hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, UsuariosDePrueba.Contrasena)
                             != PasswordVerificationResult.Failed;

            if (!vigente)
            {
                usuario.PasswordHash = hasher.HashPassword(usuario, UsuariosDePrueba.Contrasena);
                usuario.SecurityStamp = Guid.NewGuid().ToString();
            }
        }
    }
}
