using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.DTOs.Catalogo;
using Application.Exceptions;
using Application.Services.Catalogo;
using Dapper;
using Domain.Entities.Catalogo;
using Microsoft.EntityFrameworkCore;
using Persistence.Common;
using Persistence.Connections;
using Persistence.Constants;
using Persistence.Context;

namespace Persistence.Services.Catalogo
{
    public class DependenciaService : IDependenciaService
    {
        private readonly MideDbContext _context;
        private readonly ISqlConnectionFactory _conexiones;

        public DependenciaService(MideDbContext context, ISqlConnectionFactory conexiones)
        {
            _context = context;
            _conexiones = conexiones;
        }

        public async Task<IReadOnlyCollection<DependenciaDto>> ListarAsync(
            Domain.Enums.AreaFuncional? areaFuncional = null,
            Domain.Enums.Institucion? institucion = null,
            CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT  d.Id,
                        d.Nombre,
                        d.Siglas,
                        d.Institucion,
                        d.AreaFuncional,
                        COUNT(s.Id) AS TotalServicios
                FROM    {0} d
                LEFT JOIN {1} s
                        ON s.DependenciaId = d.Id AND s.IsDeleted = 0
                WHERE   d.IsDeleted = 0
                        AND (@areaFuncional IS NULL OR d.AreaFuncional = @areaFuncional)
                        AND (@institucion IS NULL OR d.Institucion = @institucion)
                GROUP BY d.Id, d.Nombre, d.Siglas, d.Institucion, d.AreaFuncional
                ORDER BY d.Nombre";

            using var conexion = _conexiones.CrearConexion();
            var filas = await conexion.QueryAsync<FilaDependencia>(
                new CommandDefinition(
                    string.Format(sql, Tablas.Dependencias, Tablas.Servicios),
                    new
                    {
                        areaFuncional = (int?)areaFuncional,
                        institucion = (int?)institucion
                    },
                    cancellationToken: cancellationToken));

            return filas.Select(f => f.ADto()).ToList();
        }

        public async Task<DependenciaDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT  d.Id,
                        d.Nombre,
                        d.Siglas,
                        d.Institucion,
                        d.AreaFuncional,
                        COUNT(s.Id) AS TotalServicios
                FROM    {0} d
                LEFT JOIN {1} s
                        ON s.DependenciaId = d.Id AND s.IsDeleted = 0
                WHERE   d.Id = @id AND d.IsDeleted = 0
                GROUP BY d.Id, d.Nombre, d.Siglas, d.Institucion, d.AreaFuncional";

            using var conexion = _conexiones.CrearConexion();
            var fila = await conexion.QuerySingleOrDefaultAsync<FilaDependencia>(
                new CommandDefinition(
                    string.Format(sql, Tablas.Dependencias, Tablas.Servicios),
                    new { id },
                    cancellationToken: cancellationToken));

            return fila?.ADto() ?? throw new NotFoundException("la dependencia", id);
        }

        public async Task<DependenciaDto> CrearAsync(GuardarDependenciaDto dto, CancellationToken cancellationToken = default)
        {
            var siglas = dto.Siglas.Trim().ToUpperInvariant();

            if (await _context.Dependencias.AnyAsync(d => d.Siglas == siglas, cancellationToken))
            {
                throw new BusinessRuleException($"Ya existe una dependencia con las siglas {siglas}.");
            }

            var dependencia = new Dependencia
            {
                Nombre = dto.Nombre.Trim(),
                Siglas = siglas,
                Institucion = dto.Institucion,
                AreaFuncional = dto.AreaFuncional
            };

            _context.Dependencias.Add(dependencia);
            await _context.SaveChangesAsync(cancellationToken);

            return Proyectar(dependencia, totalServicios: 0);
        }

        public async Task<DependenciaDto> ActualizarAsync(int id, GuardarDependenciaDto dto, CancellationToken cancellationToken = default)
        {
            var dependencia = await ObtenerParaEscrituraAsync(id, cancellationToken);
            var siglas = dto.Siglas.Trim().ToUpperInvariant();

            if (await _context.Dependencias.AnyAsync(d => d.Siglas == siglas && d.Id != id, cancellationToken))
            {
                throw new BusinessRuleException($"Ya existe otra dependencia con las siglas {siglas}.");
            }

            dependencia.Nombre = dto.Nombre.Trim();
            dependencia.Siglas = siglas;
            dependencia.Institucion = dto.Institucion;
            dependencia.AreaFuncional = dto.AreaFuncional;

            await _context.SaveChangesAsync(cancellationToken);

            var totalServicios = await _context.Servicios.CountAsync(s => s.DependenciaId == id, cancellationToken);
            return Proyectar(dependencia, totalServicios);
        }

        public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
        {
            var dependencia = await ObtenerParaEscrituraAsync(id, cancellationToken);

            var tieneServicios = await _context.Servicios.AnyAsync(s => s.DependenciaId == id, cancellationToken);
            if (tieneServicios)
            {
                throw new BusinessRuleException(
                    "No se puede eliminar una dependencia con servicios activos. Retire o reasigne los servicios primero.");
            }

            dependencia.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<Dependencia> ObtenerParaEscrituraAsync(int id, CancellationToken cancellationToken)
        {
            var dependencia = await _context.Dependencias.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
            return dependencia ?? throw new NotFoundException("la dependencia", id);
        }

        private static DependenciaDto Proyectar(Dependencia dependencia, int totalServicios) => new(
            dependencia.Id,
            dependencia.Nombre,
            dependencia.Siglas,
            dependencia.Institucion,
            Descripciones.De(dependencia.Institucion),
            dependencia.AreaFuncional,
            Descripciones.De(dependencia.AreaFuncional),
            totalServicios);

        private sealed class FilaDependencia
        {
            public int Id { get; init; }
            public string Nombre { get; init; } = string.Empty;
            public string Siglas { get; init; } = string.Empty;
            public Domain.Enums.Institucion Institucion { get; init; }
            public Domain.Enums.AreaFuncional AreaFuncional { get; init; }
            public int TotalServicios { get; init; }

            public DependenciaDto ADto() => new(
                Id,
                Nombre,
                Siglas,
                Institucion,
                Descripciones.De(Institucion),
                AreaFuncional,
                Descripciones.De(AreaFuncional),
                TotalServicios);
        }
    }
}
