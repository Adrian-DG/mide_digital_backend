using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Tramites;

namespace Application.Services.Tramites
{
    public interface ISolicitudService
    {
        Task<PagedResult<SolicitudListaDto>> ConsultarAsync(ConsultarSolicitudesDto criterios, CancellationToken cancellationToken = default);

        Task<SolicitudDetalleDto> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default);

        Task<SolicitudDetalleDto> CrearAsync(CrearSolicitudDto dto, CancellationToken cancellationToken = default);

        Task<DocumentoSolicitudDto> AdjuntarDocumentoAsync(int solicitudId, AdjuntarDocumentoDto dto, CancellationToken cancellationToken = default);

        Task EliminarDocumentoAsync(int solicitudId, int documentoId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Avanza el tramite. Rechaza las transiciones invalidas y exige que los
        /// requisitos obligatorios esten cubiertos antes de aprobar.
        /// </summary>
        Task<SolicitudDetalleDto> CambiarEstadoAsync(int solicitudId, CambiarEstadoSolicitudDto dto, Guid usuarioResponsableId, CancellationToken cancellationToken = default);

        /// <summary>Requisitos obligatorios del servicio que aun no tienen documento cargado.</summary>
        Task<IReadOnlyCollection<string>> ListarRequisitosPendientesAsync(int solicitudId, CancellationToken cancellationToken = default);

        Task EliminarAsync(int id, CancellationToken cancellationToken = default);
    }
}
