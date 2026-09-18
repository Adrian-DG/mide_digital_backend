using System;
using System.Collections.Generic;
using Application.Common;
using Domain.Enums;

namespace Application.DTOs.Catalogo
{
    /// <summary>Fila del buscador y del directorio categorizado.</summary>
    public record ServicioListaDto(
        int Id,
        string Nombre,
        string Descripcion,
        bool EstaDisponible,
        int CategoriaServicioId,
        string CategoriaNombre,
        int DependenciaId,
        string DependenciaNombre,
        string DependenciaSiglas,
        double PromedioEstrellas,
        int TotalCalificaciones);

    /// <summary>Detalle del servicio con su lista de requisitos.</summary>
    public record ServicioDetalleDto
    {
        public int Id { get; init; }
        public string Nombre { get; init; } = string.Empty;
        public string Descripcion { get; init; } = string.Empty;
        public bool EstaDisponible { get; init; }
        public string Secuencia { get; init; } = string.Empty;

        public int CategoriaServicioId { get; init; }
        public string CategoriaNombre { get; init; } = string.Empty;

        public int DependenciaId { get; init; }
        public string DependenciaNombre { get; init; } = string.Empty;
        public string DependenciaSiglas { get; init; } = string.Empty;
        public AreaFuncional AreaFuncional { get; init; }

        public double PromedioEstrellas { get; init; }
        public int TotalCalificaciones { get; init; }

        public IReadOnlyCollection<RequisitoDto> Requisitos { get; init; } = Array.Empty<RequisitoDto>();
    }

    public record GuardarServicioDto
    {
        public required string Nombre { get; init; }
        public required string Descripcion { get; init; }
        public bool EstaDisponible { get; init; } = true;
        public int CategoriaServicioId { get; init; }
        public int DependenciaId { get; init; }
    }

    /// <summary>Criterios del buscador inteligente.</summary>
    public class BuscarServiciosDto : ConsultaPaginada
    {
        public string? Termino { get; set; }
        public int? CategoriaServicioId { get; set; }
        public int? DependenciaId { get; set; }
        public AreaFuncional? AreaFuncional { get; set; }
        public bool SoloDisponibles { get; set; } = true;
    }
}
