using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Catalogo;

namespace Application.Services.Catalogo
{
    public interface ICategoriaServicioService
    {
        Task<IReadOnlyCollection<CategoriaServicioDto>> ListarAsync(CancellationToken cancellationToken = default);

        Task<CategoriaServicioDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

        Task<CategoriaServicioDto> CrearAsync(GuardarCategoriaServicioDto dto, CancellationToken cancellationToken = default);

        Task<CategoriaServicioDto> ActualizarAsync(int id, GuardarCategoriaServicioDto dto, CancellationToken cancellationToken = default);

        Task EliminarAsync(int id, CancellationToken cancellationToken = default);
    }
}
