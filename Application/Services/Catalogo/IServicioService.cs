using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Catalogo;

namespace Application.Services.Catalogo
{
    public interface IServicioService
    {
        /// <summary>Buscador inteligente: termino libre mas filtros de categoria, dependencia y area.</summary>
        Task<PagedResult<ServicioListaDto>> BuscarAsync(BuscarServiciosDto criterios, CancellationToken cancellationToken = default);

        Task<ServicioDetalleDto> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<RequisitoDto>> ListarRequisitosAsync(int servicioId, CancellationToken cancellationToken = default);

        Task<ServicioListaDto> CrearAsync(GuardarServicioDto dto, CancellationToken cancellationToken = default);

        Task<ServicioListaDto> ActualizarAsync(int id, GuardarServicioDto dto, CancellationToken cancellationToken = default);

        Task EliminarAsync(int id, CancellationToken cancellationToken = default);

        Task<RequisitoDto> AgregarRequisitoAsync(GuardarRequisitoDto dto, CancellationToken cancellationToken = default);

        Task<RequisitoDto> ActualizarRequisitoAsync(int requisitoId, GuardarRequisitoDto dto, CancellationToken cancellationToken = default);

        Task EliminarRequisitoAsync(int requisitoId, CancellationToken cancellationToken = default);
    }
}
