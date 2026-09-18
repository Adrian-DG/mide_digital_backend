using System;
using Domain.Enums;

namespace Application.DTOs.Identidad
{
    public record UsuarioDto(
        int Id,
        string Nombres,
        string Apellidos,
        string Correo,
        TipoUsuario TipoUsuario,
        string? Cedula,
        string? Telefono,
        bool CorreoConfirmado,
        DateTime FechaRegistro)
    {
        public string NombreCompleto => $"{Nombres} {Apellidos}";
    }

    public record RegistrarUsuarioDto
    {
        public required string Nombres { get; init; }
        public required string Apellidos { get; init; }
        public required string Correo { get; init; }
        public required string Contrasena { get; init; }
        public TipoUsuario TipoUsuario { get; init; }
        public string? Cedula { get; init; }
        public string? Telefono { get; init; }
    }

    public record ActualizarPerfilDto
    {
        public required string Nombres { get; init; }
        public required string Apellidos { get; init; }
        public string? Cedula { get; init; }
        public string? Telefono { get; init; }
    }

    public record CredencialesDto
    {
        public required string Correo { get; init; }
        public required string Contrasena { get; init; }
    }
}
