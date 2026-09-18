using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Retroalimentacion;
using Application.Exceptions;
using Application.Services.Retroalimentacion;
using Dapper;
using Domain.Entities.Retroalimentacion;
using Microsoft.EntityFrameworkCore;
using Persistence.Connections;
using Persistence.Constants;
using Persistence.Context;

namespace Persistence.Services.Retroalimentacion
{
    public class CalificacionServicioService : ICalificacionServicioService
    {
        private static readonly string ColumnasCalificacion = $@"
            SELECT  cal.Id,
                    cal.Estrellas,
                    cal.Comentario,
                    cal.FechaCalificacion,
                    cal.ServicioId,
                    s.Nombre AS ServicioNombre,
                    cal.UsuarioId,
                    u.Nombres + ' ' + u.Apellidos AS UsuarioNombre
            FROM    {Tablas.CalificacionesServicio} cal
            INNER JOIN {Tablas.Servicios} s ON s.Id = cal.ServicioId
            INNER JOIN {Tablas.Usuarios} u ON u.Id = cal.UsuarioId";

        private readonly MideDbContext _context;
        private readonly ISqlConnectionFactory _conexiones;

        public CalificacionServicioService(MideDbContext context, ISqlConnectionFactory conexiones)
        {
            _context = context;
            _conexiones = conexiones;
        }

        public async Task<CalificacionServicioDto> CalificarAsync(CrearCalificacionDto dto, CancellationToken cancellationToken = default)
        {
            if (dto.Estrellas < CalificacionServicio.EstrellasMinimas || dto.Estrellas > CalificacionServicio.EstrellasMaximas)
            {
                throw new BusinessRuleException(
                    $"La calificación debe estar entre {CalificacionServicio.EstrellasMinimas} y {CalificacionServicio.EstrellasMaximas} estrellas.");
            }

            var existeServicio = await _context.Servicios.AnyAsync(s => s.Id == dto.ServicioId, cancellationToken);
            if (!existeServicio)
            {
                throw new NotFoundException("el servicio", dto.ServicioId);
            }

            var existeUsuario = await _context.Usuarios.AnyAsync(u => u.Id == dto.UsuarioId, cancellationToken);
            if (!existeUsuario)
            {
                throw new NotFoundException("el usuario", dto.UsuarioId);
            }

            // Una calificación por usuario y servicio: la nueva reemplaza la anterior,
            // que es lo que espera el módulo de retroalimentación continua.
            var calificacion = await _context.CalificacionesServicio
                .FirstOrDefaultAsync(c => c.ServicioId == dto.ServicioId && c.UsuarioId == dto.UsuarioId, cancellationToken);

            if (calificacion is null)
            {
                calificacion = new CalificacionServicio
                {
                    ServicioId = dto.ServicioId,
                    UsuarioId = dto.UsuarioId
                };

                _context.CalificacionesServicio.Add(calificacion);
            }

            calificacion.Estrellas = dto.Estrellas;
            calificacion.Comentario = string.IsNullOrWhiteSpace(dto.Comentario) ? null : dto.Comentario.Trim();
            calificacion.FechaCalificacion = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return await ObtenerPorIdAsync(calificacion.Id, cancellationToken);
        }

        public async Task<PagedResult<CalificacionServicioDto>> ListarPorServicioAsync(
            int servicioId,
            ConsultaPaginada paginacion,
            CancellationToken cancellationToken = default)
        {
            var sqlDatos = $@"{ColumnasCalificacion}
                WHERE   cal.ServicioId = @servicioId AND cal.IsDeleted = 0
                ORDER BY cal.FechaCalificacion DESC
                OFFSET @offset ROWS FETCH NEXT @tamanoPagina ROWS ONLY";

            var sqlTotal = $@"
                SELECT  COUNT(1)
                FROM    {Tablas.CalificacionesServicio}
                WHERE   ServicioId = @servicioId AND IsDeleted = 0";

            var parametros = new
            {
                servicioId,
                offset = paginacion.Offset,
                tamanoPagina = paginacion.TamanoPagina
            };

            using var conexion = _conexiones.CrearConexion();

            var items = await conexion.QueryAsync<CalificacionServicioDto>(
                new CommandDefinition(sqlDatos, parametros, cancellationToken: cancellationToken));

            var total = await conexion.ExecuteScalarAsync<int>(
                new CommandDefinition(sqlTotal, parametros, cancellationToken: cancellationToken));

            return new PagedResult<CalificacionServicioDto>
            {
                Items = items.ToList(),
                TotalRegistros = total,
                Pagina = paginacion.Pagina,
                TamanoPagina = paginacion.TamanoPagina
            };
        }

        public async Task<IReadOnlyCollection<CalificacionServicioDto>> ListarPorUsuarioAsync(int usuarioId, CancellationToken cancellationToken = default)
        {
            var sql = $@"{ColumnasCalificacion}
                WHERE   cal.UsuarioId = @usuarioId AND cal.IsDeleted = 0
                ORDER BY cal.FechaCalificacion DESC";

            using var conexion = _conexiones.CrearConexion();
            var calificaciones = await conexion.QueryAsync<CalificacionServicioDto>(
                new CommandDefinition(sql, new { usuarioId }, cancellationToken: cancellationToken));

            return calificaciones.ToList();
        }

        public async Task<ResumenCalificacionesDto> ObtenerResumenAsync(int servicioId, CancellationToken cancellationToken = default)
        {
            var sqlServicio = $"SELECT Nombre FROM {Tablas.Servicios} WHERE Id = @servicioId AND IsDeleted = 0";

            var sqlDistribucion = $@"
                SELECT  Estrellas, COUNT(1) AS Cantidad
                FROM    {Tablas.CalificacionesServicio}
                WHERE   ServicioId = @servicioId AND IsDeleted = 0
                GROUP BY Estrellas";

            using var conexion = _conexiones.CrearConexion();

            var nombreServicio = await conexion.QuerySingleOrDefaultAsync<string>(
                new CommandDefinition(sqlServicio, new { servicioId }, cancellationToken: cancellationToken));

            if (nombreServicio is null)
            {
                throw new NotFoundException("el servicio", servicioId);
            }

            var filas = await conexion.QueryAsync<FilaDistribucion>(
                new CommandDefinition(sqlDistribucion, new { servicioId }, cancellationToken: cancellationToken));

            var distribucion = Enumerable
                .Range(
                    CalificacionServicio.EstrellasMinimas,
                    CalificacionServicio.EstrellasMaximas - CalificacionServicio.EstrellasMinimas + 1)
                .ToDictionary(estrellas => estrellas, _ => 0);

            var total = 0;
            var suma = 0;

            foreach (var fila in filas)
            {
                distribucion[fila.Estrellas] = fila.Cantidad;
                total += fila.Cantidad;
                suma += fila.Estrellas * fila.Cantidad;
            }

            return new ResumenCalificacionesDto
            {
                ServicioId = servicioId,
                ServicioNombre = nombreServicio,
                TotalCalificaciones = total,
                PromedioEstrellas = total == 0 ? 0 : Math.Round(suma / (double)total, 2),
                DistribucionPorEstrellas = distribucion
            };
        }

        public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
        {
            var calificacion = await _context.CalificacionesServicio
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
                ?? throw new NotFoundException("la calificación", id);

            calificacion.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<CalificacionServicioDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken)
        {
            var sql = $@"{ColumnasCalificacion}
                WHERE   cal.Id = @id AND cal.IsDeleted = 0";

            using var conexion = _conexiones.CrearConexion();
            var calificacion = await conexion.QuerySingleOrDefaultAsync<CalificacionServicioDto>(
                new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));

            return calificacion ?? throw new NotFoundException("la calificación", id);
        }

        private sealed class FilaDistribucion
        {
            public int Estrellas { get; init; }
            public int Cantidad { get; init; }
        }
    }
}
