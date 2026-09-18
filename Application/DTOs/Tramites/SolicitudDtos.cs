using System;
using System.Collections.Generic;
using Application.Common;
using Domain.Enums;

namespace Application.DTOs.Tramites
{
    public record SolicitudListaDto(
        int Id,
        string Secuencia,
        EstadoSolicitud Estado,
        DateTime CreatedAt,
        DateTime? FechaProcesamiento,
        int ServicioId,
        string ServicioNombre,
        string DependenciaSiglas,
        Guid UsuarioId,
        string UsuarioNombre,
        int TotalDocumentos);

    public record SolicitudDetalleDto
    {
        public int Id { get; init; }
        public string Secuencia { get; init; } = string.Empty;
        public EstadoSolicitud Estado { get; init; }
        public string? Observaciones { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? FechaProcesamiento { get; init; }

        public int ServicioId { get; init; }
        public string ServicioNombre { get; init; } = string.Empty;
        public int DependenciaId { get; init; }
        public string DependenciaNombre { get; init; } = string.Empty;

        public Guid UsuarioId { get; init; }
        public string UsuarioNombre { get; init; } = string.Empty;
        public string UsuarioCorreo { get; init; } = string.Empty;

        public IReadOnlyCollection<DocumentoSolicitudDto> Documentos { get; init; }
            = Array.Empty<DocumentoSolicitudDto>();
    }

    public record CrearSolicitudDto
    {
        public int ServicioId { get; init; }
        public Guid UsuarioId { get; init; }
        public string? Observaciones { get; init; }
    }

    public record CambiarEstadoSolicitudDto
    {
        public EstadoSolicitud NuevoEstado { get; init; }
        public string? Observaciones { get; init; }
    }

    public class ConsultarSolicitudesDto : ConsultaPaginada
    {
        public Guid? UsuarioId { get; set; }
        public int? ServicioId { get; set; }
        public int? DependenciaId { get; set; }
        public EstadoSolicitud? Estado { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Hasta { get; set; }
    }
}
