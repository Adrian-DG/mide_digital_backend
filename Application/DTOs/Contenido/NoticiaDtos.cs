using System;
using Application.Common;

namespace Application.DTOs.Contenido
{
    public record NoticiaListaDto(
        int Id,
        string Titulo,
        string Resumen,
        string? ImagenUrl,
        string Fuente,
        DateTime FechaPublicacion,
        bool EsDestacada);

    public record NoticiaDetalleDto(
        int Id,
        string Titulo,
        string Resumen,
        string Contenido,
        string? ImagenUrl,
        string Fuente,
        DateTime FechaPublicacion,
        bool EsDestacada);

    public record GuardarNoticiaDto
    {
        public required string Titulo { get; init; }
        public required string Resumen { get; init; }
        public required string Contenido { get; init; }
        public string? ImagenUrl { get; init; }
        public required string Fuente { get; init; }
        public DateTime FechaPublicacion { get; init; }
        public bool EsDestacada { get; init; }
    }

    public class ConsultarNoticiasDto : ConsultaPaginada
    {
        public string? Termino { get; set; }
        public bool? SoloDestacadas { get; set; }
    }
}
