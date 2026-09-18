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
using Persistence.Connections;
using Persistence.Constants;
using Persistence.Context;

namespace Persistence.Services.Catalogo
{
    public class CategoriaServicioService : ICategoriaServicioService
    {
        private readonly MideDbContext _context;
        private readonly ISqlConnectionFactory _conexiones;

        public CategoriaServicioService(MideDbContext context, ISqlConnectionFactory conexiones)
        {
            _context = context;
            _conexiones = conexiones;
        }

        public async Task<IReadOnlyCollection<CategoriaServicioDto>> ListarAsync(CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT  c.Id,
                        c.Nombre,
                        c.Descripcion,
                        COUNT(s.Id) AS TotalServicios
                FROM    {0} c
                LEFT JOIN {1} s
                        ON s.CategoriaServicioId = c.Id AND s.IsDeleted = 0
                WHERE   c.IsDeleted = 0
                GROUP BY c.Id, c.Nombre, c.Descripcion
                ORDER BY c.Nombre";

            using var conexion = _conexiones.CrearConexion();
            var categorias = await conexion.QueryAsync<CategoriaServicioDto>(
                new CommandDefinition(
                    string.Format(sql, Tablas.CategoriasServicio, Tablas.Servicios),
                    cancellationToken: cancellationToken));

            return categorias.ToList();
        }

        public async Task<CategoriaServicioDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            const string sql = @"
                SELECT  c.Id,
                        c.Nombre,
                        c.Descripcion,
                        COUNT(s.Id) AS TotalServicios
                FROM    {0} c
                LEFT JOIN {1} s
                        ON s.CategoriaServicioId = c.Id AND s.IsDeleted = 0
                WHERE   c.Id = @id AND c.IsDeleted = 0
                GROUP BY c.Id, c.Nombre, c.Descripcion";

            using var conexion = _conexiones.CrearConexion();
            var categoria = await conexion.QuerySingleOrDefaultAsync<CategoriaServicioDto>(
                new CommandDefinition(
                    string.Format(sql, Tablas.CategoriasServicio, Tablas.Servicios),
                    new { id },
                    cancellationToken: cancellationToken));

            return categoria ?? throw new NotFoundException("la categoría de servicio", id);
        }

        public async Task<CategoriaServicioDto> CrearAsync(GuardarCategoriaServicioDto dto, CancellationToken cancellationToken = default)
        {
            var nombre = dto.Nombre.Trim();

            if (await _context.CategoriasServicio.AnyAsync(c => c.Nombre == nombre, cancellationToken))
            {
                throw new BusinessRuleException($"Ya existe una categoría llamada {nombre}.");
            }

            var categoria = new CategoriaServicio
            {
                Nombre = nombre,
                Descripcion = dto.Descripcion.Trim()
            };

            _context.CategoriasServicio.Add(categoria);
            await _context.SaveChangesAsync(cancellationToken);

            return new CategoriaServicioDto(categoria.Id, categoria.Nombre, categoria.Descripcion, 0);
        }

        public async Task<CategoriaServicioDto> ActualizarAsync(int id, GuardarCategoriaServicioDto dto, CancellationToken cancellationToken = default)
        {
            var categoria = await ObtenerParaEscrituraAsync(id, cancellationToken);
            var nombre = dto.Nombre.Trim();

            if (await _context.CategoriasServicio.AnyAsync(c => c.Nombre == nombre && c.Id != id, cancellationToken))
            {
                throw new BusinessRuleException($"Ya existe otra categoría llamada {nombre}.");
            }

            categoria.Nombre = nombre;
            categoria.Descripcion = dto.Descripcion.Trim();

            await _context.SaveChangesAsync(cancellationToken);

            var totalServicios = await _context.Servicios.CountAsync(s => s.CategoriaServicioId == id, cancellationToken);
            return new CategoriaServicioDto(categoria.Id, categoria.Nombre, categoria.Descripcion, totalServicios);
        }

        public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
        {
            var categoria = await ObtenerParaEscrituraAsync(id, cancellationToken);

            if (await _context.Servicios.AnyAsync(s => s.CategoriaServicioId == id, cancellationToken))
            {
                throw new BusinessRuleException(
                    "No se puede eliminar una categoría con servicios asociados. Reclasifique los servicios primero.");
            }

            categoria.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<CategoriaServicio> ObtenerParaEscrituraAsync(int id, CancellationToken cancellationToken)
        {
            var categoria = await _context.CategoriasServicio.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            return categoria ?? throw new NotFoundException("la categoría de servicio", id);
        }
    }
}
