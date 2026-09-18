using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Retroalimentacion;

namespace Application.Services.Retroalimentacion
{
    public interface ICalificacionServicioService
    {
        Task<CalificacionServicioDto> CalificarAsync(CrearCalificacionDto dto, CancellationToken cancellationToken = default);

        Task<PagedResult<CalificacionServicioDto>> ListarPorServicioAsync(int servicioId, ConsultaPaginada paginacion, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<CalificacionServicioDto>> ListarPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);

        Task<ResumenCalificacionesDto> ObtenerResumenAsync(int servicioId, CancellationToken cancellationToken = default);

        Task EliminarAsync(int id, CancellationToken cancellationToken = default);
    }
}
