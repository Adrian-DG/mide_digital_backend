namespace Application.DTOs.Catalogo
{
    public record RequisitoDto(
        int Id,
        string Nombre,
        string Descripcion,
        bool EsObligatorio,
        int ServicioId);

    public record GuardarRequisitoDto
    {
        public required string Nombre { get; init; }
        public required string Descripcion { get; init; }
        public bool EsObligatorio { get; init; }
        public int ServicioId { get; init; }
    }
}
