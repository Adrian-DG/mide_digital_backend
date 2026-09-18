using Domain.Enums;

namespace Application.DTOs.Catalogo
{
    public record DependenciaDto(
        int Id,
        string Nombre,
        string Siglas,
        Institucion Institucion,
        string InstitucionDescripcion,
        AreaFuncional AreaFuncional,
        string AreaFuncionalDescripcion,
        int TotalServicios);

    public record GuardarDependenciaDto
    {
        public required string Nombre { get; init; }
        public required string Siglas { get; init; }
        public Institucion Institucion { get; init; }
        public AreaFuncional AreaFuncional { get; init; }
    }
}
