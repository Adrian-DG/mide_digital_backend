using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Catalogo;
using Domain.Enums;

namespace Application.Services.Catalogo
{
    public interface IDependenciaService
    {
        Task<IReadOnlyCollection<DependenciaDto>> ListarAsync(AreaFuncional? areaFuncional = null, Institucion? institucion = null, CancellationToken cancellationToken = default);

        Task<DependenciaDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

        Task<DependenciaDto> CrearAsync(GuardarDependenciaDto dto, CancellationToken cancellationToken = default);

        Task<DependenciaDto> ActualizarAsync(int id, GuardarDependenciaDto dto, CancellationToken cancellationToken = default);

        Task EliminarAsync(int id, CancellationToken cancellationToken = default);
    }
}
