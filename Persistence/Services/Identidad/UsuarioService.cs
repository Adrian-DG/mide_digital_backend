using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstraction;
using Application.DTOs.Identidad;
using Application.Exceptions;
using Application.Services.Identidad;
using Dapper;
using Domain.Entities.Identidad;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Connections;
using Persistence.Constants;
using Persistence.Context;

namespace Persistence.Services.Identidad
{
    public class UsuarioService : IUsuarioService
    {
        private const string ColumnasUsuario =
            "Id, Nombres, Apellidos, Correo, TipoUsuario, Cedula, Telefono, CorreoConfirmado, FechaRegistro";

        private readonly MideDbContext _context;
        private readonly ISqlConnectionFactory _conexiones;
        private readonly IPasswordHasher _passwordHasher;

        public UsuarioService(MideDbContext context, ISqlConnectionFactory conexiones, IPasswordHasher passwordHasher)
        {
            _context = context;
            _conexiones = conexiones;
            _passwordHasher = passwordHasher;
        }

        public async Task<UsuarioDto> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken cancellationToken = default)
        {
            var correo = dto.Correo.Trim().ToLowerInvariant();

            var correoEnUso = await _context.Usuarios
                .AnyAsync(u => u.Correo == correo, cancellationToken);

            if (correoEnUso)
            {
                throw new BusinessRuleException($"El correo {correo} ya está registrado.");
            }

            var usuario = new Usuario
            {
                Nombres = dto.Nombres.Trim(),
                Apellidos = dto.Apellidos.Trim(),
                Correo = correo,
                ContrasenaHash = _passwordHasher.Hash(dto.Contrasena),
                TipoUsuario = dto.TipoUsuario,
                Cedula = Normalizar(dto.Cedula),
                Telefono = Normalizar(dto.Telefono),
                CorreoConfirmado = false,
                FechaRegistro = DateTime.UtcNow
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return Proyectar(usuario);
        }

        public async Task<UsuarioDto?> AutenticarAsync(CredencialesDto dto, CancellationToken cancellationToken = default)
        {
            var correo = dto.Correo.Trim().ToLowerInvariant();

            using var conexion = _conexiones.CrearConexion();
            var fila = await conexion.QuerySingleOrDefaultAsync<FilaCredenciales>(
                new CommandDefinition(
                    $"SELECT {ColumnasUsuario}, ContrasenaHash FROM {Tablas.Usuarios} WHERE Correo = @correo AND IsDeleted = 0",
                    new { correo },
                    cancellationToken: cancellationToken));

            if (fila is null || !_passwordHasher.Verificar(dto.Contrasena, fila.ContrasenaHash))
            {
                return null;
            }

            return fila.AUsuarioDto();
        }

        public async Task<UsuarioDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            using var conexion = _conexiones.CrearConexion();
            var usuario = await conexion.QuerySingleOrDefaultAsync<UsuarioDto>(
                new CommandDefinition(
                    $"SELECT {ColumnasUsuario} FROM {Tablas.Usuarios} WHERE Id = @id AND IsDeleted = 0",
                    new { id },
                    cancellationToken: cancellationToken));

            return usuario ?? throw new NotFoundException("el usuario", id);
        }

        public async Task<UsuarioDto?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default)
        {
            var normalizado = correo.Trim().ToLowerInvariant();

            using var conexion = _conexiones.CrearConexion();
            return await conexion.QuerySingleOrDefaultAsync<UsuarioDto>(
                new CommandDefinition(
                    $"SELECT {ColumnasUsuario} FROM {Tablas.Usuarios} WHERE Correo = @normalizado AND IsDeleted = 0",
                    new { normalizado },
                    cancellationToken: cancellationToken));
        }

        public async Task<UsuarioDto> ActualizarPerfilAsync(int id, ActualizarPerfilDto dto, CancellationToken cancellationToken = default)
        {
            var usuario = await ObtenerParaEscrituraAsync(id, cancellationToken);

            usuario.Nombres = dto.Nombres.Trim();
            usuario.Apellidos = dto.Apellidos.Trim();
            usuario.Cedula = Normalizar(dto.Cedula);
            usuario.Telefono = Normalizar(dto.Telefono);

            await _context.SaveChangesAsync(cancellationToken);

            return Proyectar(usuario);
        }

        public async Task CambiarContrasenaAsync(int id, string contrasenaActual, string contrasenaNueva, CancellationToken cancellationToken = default)
        {
            var usuario = await ObtenerParaEscrituraAsync(id, cancellationToken);

            if (!_passwordHasher.Verificar(contrasenaActual, usuario.ContrasenaHash))
            {
                throw new BusinessRuleException("La contraseña actual no es correcta.");
            }

            usuario.ContrasenaHash = _passwordHasher.Hash(contrasenaNueva);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task ConfirmarCorreoAsync(int id, CancellationToken cancellationToken = default)
        {
            var usuario = await ObtenerParaEscrituraAsync(id, cancellationToken);

            usuario.CorreoConfirmado = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
        {
            var usuario = await ObtenerParaEscrituraAsync(id, cancellationToken);

            usuario.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<Usuario> ObtenerParaEscrituraAsync(int id, CancellationToken cancellationToken)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
            return usuario ?? throw new NotFoundException("el usuario", id);
        }

        private static string? Normalizar(string? valor)
            => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        private static UsuarioDto Proyectar(Usuario usuario) => new(
            usuario.Id,
            usuario.Nombres,
            usuario.Apellidos,
            usuario.Correo,
            usuario.TipoUsuario,
            usuario.Cedula,
            usuario.Telefono,
            usuario.CorreoConfirmado,
            usuario.FechaRegistro);

        /// <summary>Fila interna de lectura: el hash no sale de esta capa.</summary>
        private sealed class FilaCredenciales
        {
            public int Id { get; init; }
            public string Nombres { get; init; } = string.Empty;
            public string Apellidos { get; init; } = string.Empty;
            public string Correo { get; init; } = string.Empty;
            public TipoUsuario TipoUsuario { get; init; }
            public string? Cedula { get; init; }
            public string? Telefono { get; init; }
            public bool CorreoConfirmado { get; init; }
            public DateTime FechaRegistro { get; init; }
            public string ContrasenaHash { get; init; } = string.Empty;

            public UsuarioDto AUsuarioDto() => new(
                Id, Nombres, Apellidos, Correo, TipoUsuario, Cedula, Telefono, CorreoConfirmado, FechaRegistro);
        }
    }
}
