using System;

namespace Application.DTOs.Tramites
{
    public record DocumentoSolicitudDto(
        int Id,
        string NombreArchivo,
        string RutaArchivo,
        string TipoContenido,
        long TamanoBytes,
        DateTime FechaCarga,
        int SolicitudId,
        int RequisitoId,
        string RequisitoNombre,
        bool RequisitoEsObligatorio);

    public record AdjuntarDocumentoDto
    {
        public int RequisitoId { get; init; }
        public required string NombreArchivo { get; init; }
        public required string RutaArchivo { get; init; }
        public required string TipoContenido { get; init; }
        public long TamanoBytes { get; init; }
    }
}
