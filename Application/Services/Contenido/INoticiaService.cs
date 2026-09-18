using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Contenido;

namespace Application.Services.Contenido
{
    public interface INoticiaService
    {
        Task<PagedResult<NoticiaListaDto>> ConsultarAsync(ConsultarNoticiasDto criterios, CancellationToken cancellationToken = default);

        Task<NoticiaDetalleDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

        Task<NoticiaDetalleDto> CrearAsync(GuardarNoticiaDto dto, CancellationToken cancellationToken = default);

        Task<NoticiaDetalleDto> ActualizarAsync(int id, GuardarNoticiaDto dto, CancellationToken cancellationToken = default);

        Task EliminarAsync(int id, CancellationToken cancellationToken = default);
    }
}
