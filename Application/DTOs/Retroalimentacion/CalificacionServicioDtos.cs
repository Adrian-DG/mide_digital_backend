using System;
using System.Collections.Generic;

namespace Application.DTOs.Retroalimentacion
{
    public record CalificacionServicioDto(
        int Id,
        int Estrellas,
        string? Comentario,
        DateTime FechaCalificacion,
        int ServicioId,
        string ServicioNombre,
        int UsuarioId,
        string UsuarioNombre);

    public record CrearCalificacionDto
    {
        public int ServicioId { get; init; }
        public int UsuarioId { get; init; }
        public int Estrellas { get; init; }
        public string? Comentario { get; init; }
    }

    /// <summary>Insumo de los reportes internos de actualización del directorio.</summary>
    public record ResumenCalificacionesDto
    {
        public int ServicioId { get; init; }
        public string ServicioNombre { get; init; } = string.Empty;
        public double PromedioEstrellas { get; init; }
        public int TotalCalificaciones { get; init; }
        public IReadOnlyDictionary<int, int> DistribucionPorEstrellas { get; init; }
            = new Dictionary<int, int>();
    }
}
