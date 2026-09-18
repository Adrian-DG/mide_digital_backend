namespace Application.DTOs.Catalogo
{
    public record CategoriaServicioDto(
        int Id,
        string Nombre,
        string Descripcion,
        int TotalServicios);

    public record GuardarCategoriaServicioDto
    {
        public required string Nombre { get; init; }
        public required string Descripcion { get; init; }
    }
}
