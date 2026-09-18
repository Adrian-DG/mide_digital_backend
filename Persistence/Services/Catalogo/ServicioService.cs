using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Catalogo;
using Application.Exceptions;
using Application.Services.Catalogo;
using Dapper;
using Domain.Entities.Catalogo;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Common;
using Persistence.Connections;
using Persistence.Constants;
using Persistence.Context;

namespace Persistence.Services.Catalogo
{
    public class ServicioService : IServicioService
    {
        /// <summary>
        /// Promedio y conteo de estrellas por servicio, como subconsulta agregada.
        /// Se une con LEFT JOIN en lugar de agrupar el conjunto completo, para que
        /// el conteo de la paginación no dependa de las calificaciones.
        /// </summary>
        private static readonly string ResumenCalificaciones = $@"
            (SELECT ServicioId,
                    AVG(CAST(Estrellas AS FLOAT)) AS Promedio,
                    COUNT(1) AS Total
             FROM   {Tablas.CalificacionesServicio}
             WHERE  IsDeleted = 0
             GROUP BY ServicioId)";

        private static readonly string OrigenBusqueda = $@"
            FROM    {Tablas.Servicios} s
            INNER JOIN {Tablas.CategoriasServicio} c ON c.Id = s.CategoriaServicioId AND c.IsDeleted = 0
            INNER JOIN {Tablas.Dependencias} d ON d.Id = s.DependenciaId AND d.IsDeleted = 0
            LEFT JOIN {ResumenCalificaciones} cal ON cal.ServicioId = s.Id
            WHERE   s.IsDeleted = 0
                    AND (@soloDisponibles = 0 OR s.EstaDisponible = 1)
                    AND (@categoriaServicioId IS NULL OR s.CategoriaServicioId = @categoriaServicioId)
                    AND (@dependenciaId IS NULL OR s.DependenciaId = @dependenciaId)
                    AND (@areaFuncional IS NULL OR d.AreaFuncional = @areaFuncional)
                    AND (@termino IS NULL
                         OR s.Nombre LIKE @patron
                         OR s.Descripcion LIKE @patron
                         OR d.Nombre LIKE @patron
                         OR d.Siglas LIKE @patron)";

        private const string ColumnasLista = @"
            SELECT  s.Id,
                    s.Nombre,
                    s.Descripcion,
                    s.EstaDisponible,
                    s.CategoriaServicioId,
                    c.Nombre AS CategoriaNombre,
                    s.DependenciaId,
                    d.Nombre AS DependenciaNombre,
                    d.Siglas AS DependenciaSiglas,
                    COALESCE(cal.Promedio, 0) AS PromedioEstrellas,
                    COALESCE(cal.Total, 0) AS TotalCalificaciones";

        private readonly MideDbContext _context;
        private readonly ISqlConnectionFactory _conexiones;

        public ServicioService(MideDbContext context, ISqlConnectionFactory conexiones)
        {
            _context = context;
            _conexiones = conexiones;
        }

        /// <summary>
        /// Buscador inteligente: un término libre contra nombre y descripción del
        /// servicio más nombre y siglas de la dependencia, de modo que "hospital"
        /// o "CAIFFAA" resuelvan por igual.
        /// </summary>
        public async Task<PagedResult<ServicioListaDto>> BuscarAsync(BuscarServiciosDto criterios, CancellationToken cancellationToken = default)
        {
            var termino = string.IsNullOrWhiteSpace(criterios.Termino) ? null : criterios.Termino.Trim();
            var parametros = new
            {
                termino,
                patron = termino is null ? null : $"%{Sql.EscaparLike(termino)}%",
                categoriaServicioId = criterios.CategoriaServicioId,
                dependenciaId = criterios.DependenciaId,
                areaFuncional = (int?)criterios.AreaFuncional,
                soloDisponibles = criterios.SoloDisponibles ? 1 : 0,
                offset = criterios.Offset,
                tamanoPagina = criterios.TamanoPagina
            };

            var sqlDatos = $@"{ColumnasLista}{OrigenBusqueda}
                ORDER BY s.Nombre
                OFFSET @offset ROWS FETCH NEXT @tamanoPagina ROWS ONLY";

            var sqlTotal = $"SELECT COUNT(1){OrigenBusqueda}";

            using var conexion = _conexiones.CrearConexion();

            var items = await conexion.QueryAsync<ServicioListaDto>(
                new CommandDefinition(sqlDatos, parametros, cancellationToken: cancellationToken));

            var total = await conexion.ExecuteScalarAsync<int>(
                new CommandDefinition(sqlTotal, parametros, cancellationToken: cancellationToken));

            return new PagedResult<ServicioListaDto>
            {
                Items = items.ToList(),
                TotalRegistros = total,
                Pagina = criterios.Pagina,
                TamanoPagina = criterios.TamanoPagina
            };
        }

        public async Task<ServicioDetalleDto> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default)
        {
            var sql = $@"
                SELECT  s.Id,
                        s.Nombre,
                        s.Descripcion,
                        s.EstaDisponible,
                        s.CategoriaServicioId,
                        c.Nombre AS CategoriaNombre,
                        s.DependenciaId,
                        d.Nombre AS DependenciaNombre,
                        d.Siglas AS DependenciaSiglas,
                        d.AreaFuncional,
                        COALESCE(cal.Promedio, 0) AS PromedioEstrellas,
                        COALESCE(cal.Total, 0) AS TotalCalificaciones
                FROM    {Tablas.Servicios} s
                INNER JOIN {Tablas.CategoriasServicio} c ON c.Id = s.CategoriaServicioId
                INNER JOIN {Tablas.Dependencias} d ON d.Id = s.DependenciaId
                LEFT JOIN {ResumenCalificaciones} cal ON cal.ServicioId = s.Id
                WHERE   s.Id = @id AND s.IsDeleted = 0";

            using var conexion = _conexiones.CrearConexion();

            var detalle = await conexion.QuerySingleOrDefaultAsync<ServicioDetalleDto>(
                new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));

            if (detalle is null)
            {
                throw new NotFoundException("el servicio", id);
            }

            var requisitos = await LeerRequisitosAsync(conexion, id, cancellationToken);

            return detalle with
            {
                Requisitos = requisitos,
                // El formato del código visible vive en el dominio, no en SQL.
                Secuencia = Servicio.GenerarSecuencia(detalle.CategoriaNombre, detalle.Nombre)
            };
        }

        public async Task<IReadOnlyCollection<RequisitoDto>> ListarRequisitosAsync(int servicioId, CancellationToken cancellationToken = default)
        {
            using var conexion = _conexiones.CrearConexion();
            return await LeerRequisitosAsync(conexion, servicioId, cancellationToken);
        }

        public async Task<ServicioListaDto> CrearAsync(GuardarServicioDto dto, CancellationToken cancellationToken = default)
        {
            await ValidarCatalogosAsync(dto, cancellationToken);

            var servicio = new Servicio
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion.Trim(),
                EstaDisponible = dto.EstaDisponible,
                CategoriaServicioId = dto.CategoriaServicioId,
                DependenciaId = dto.DependenciaId
            };

            _context.Servicios.Add(servicio);
            await _context.SaveChangesAsync(cancellationToken);

            return await ProyectarListaAsync(servicio.Id, cancellationToken);
        }

        public async Task<ServicioListaDto> ActualizarAsync(int id, GuardarServicioDto dto, CancellationToken cancellationToken = default)
        {
            var servicio = await ObtenerParaEscrituraAsync(id, cancellationToken);
            await ValidarCatalogosAsync(dto, cancellationToken);

            servicio.Nombre = dto.Nombre.Trim();
            servicio.Descripcion = dto.Descripcion.Trim();
            servicio.EstaDisponible = dto.EstaDisponible;
            servicio.CategoriaServicioId = dto.CategoriaServicioId;
            servicio.DependenciaId = dto.DependenciaId;

            await _context.SaveChangesAsync(cancellationToken);

            return await ProyectarListaAsync(id, cancellationToken);
        }

        public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
        {
            var servicio = await ObtenerParaEscrituraAsync(id, cancellationToken);

            var tieneTramitesAbiertos = await _context.Solicitudes.AnyAsync(
                s => s.ServicioId == id
                     && (s.Estado == EstadoSolicitud.PENDIENTE || s.Estado == EstadoSolicitud.EN_PROCESO),
                cancellationToken);

            if (tieneTramitesAbiertos)
            {
                throw new BusinessRuleException(
                    "El servicio tiene trámites en curso. Ciérrelos, o márquelo como no disponible en lugar de eliminarlo.");
            }

            servicio.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<RequisitoDto> AgregarRequisitoAsync(GuardarRequisitoDto dto, CancellationToken cancellationToken = default)
        {
            var existeServicio = await _context.Servicios.AnyAsync(s => s.Id == dto.ServicioId, cancellationToken);
            if (!existeServicio)
            {
                throw new NotFoundException("el servicio", dto.ServicioId);
            }

            var requisito = new Requisito
            {
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion.Trim(),
                EsObligatorio = dto.EsObligatorio,
                ServicioId = dto.ServicioId
            };

            _context.Requisitos.Add(requisito);
            await _context.SaveChangesAsync(cancellationToken);

            return Proyectar(requisito);
        }

        public async Task<RequisitoDto> ActualizarRequisitoAsync(int requisitoId, GuardarRequisitoDto dto, CancellationToken cancellationToken = default)
        {
            var requisito = await ObtenerRequisitoAsync(requisitoId, cancellationToken);

            requisito.Nombre = dto.Nombre.Trim();
            requisito.Descripcion = dto.Descripcion.Trim();
            requisito.EsObligatorio = dto.EsObligatorio;

            await _context.SaveChangesAsync(cancellationToken);

            return Proyectar(requisito);
        }

        public async Task EliminarRequisitoAsync(int requisitoId, CancellationToken cancellationToken = default)
        {
            var requisito = await ObtenerRequisitoAsync(requisitoId, cancellationToken);

            var tieneDocumentos = await _context.DocumentosSolicitud.AnyAsync(d => d.RequisitoId == requisitoId, cancellationToken);
            if (tieneDocumentos)
            {
                throw new BusinessRuleException(
                    "El requisito ya tiene documentos cargados en trámites existentes y no puede eliminarse.");
            }

            requisito.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private static async Task<IReadOnlyCollection<RequisitoDto>> LeerRequisitosAsync(
            IDbConnection conexion,
            int servicioId,
            CancellationToken cancellationToken)
        {
            var sql = $@"
                SELECT  Id, Nombre, Descripcion, EsObligatorio, ServicioId
                FROM    {Tablas.Requisitos}
                WHERE   ServicioId = @servicioId AND IsDeleted = 0
                ORDER BY EsObligatorio DESC, Nombre";

            var requisitos = await conexion.QueryAsync<RequisitoDto>(
                new CommandDefinition(sql, new { servicioId }, cancellationToken: cancellationToken));

            return requisitos.ToList();
        }

        private async Task<ServicioListaDto> ProyectarListaAsync(int id, CancellationToken cancellationToken)
        {
            var sql = $@"{ColumnasLista}
                FROM    {Tablas.Servicios} s
                INNER JOIN {Tablas.CategoriasServicio} c ON c.Id = s.CategoriaServicioId
                INNER JOIN {Tablas.Dependencias} d ON d.Id = s.DependenciaId
                LEFT JOIN {ResumenCalificaciones} cal ON cal.ServicioId = s.Id
                WHERE   s.Id = @id AND s.IsDeleted = 0";

            using var conexion = _conexiones.CrearConexion();
            var servicio = await conexion.QuerySingleOrDefaultAsync<ServicioListaDto>(
                new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));

            return servicio ?? throw new NotFoundException("el servicio", id);
        }

        private async Task ValidarCatalogosAsync(GuardarServicioDto dto, CancellationToken cancellationToken)
        {
            var existeCategoria = await _context.CategoriasServicio.AnyAsync(c => c.Id == dto.CategoriaServicioId, cancellationToken);
            if (!existeCategoria)
            {
                throw new NotFoundException("la categoría de servicio", dto.CategoriaServicioId);
            }

            var existeDependencia = await _context.Dependencias.AnyAsync(d => d.Id == dto.DependenciaId, cancellationToken);
            if (!existeDependencia)
            {
                throw new NotFoundException("la dependencia", dto.DependenciaId);
            }
        }

        private async Task<Servicio> ObtenerParaEscrituraAsync(int id, CancellationToken cancellationToken)
        {
            var servicio = await _context.Servicios.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            return servicio ?? throw new NotFoundException("el servicio", id);
        }

        private async Task<Requisito> ObtenerRequisitoAsync(int requisitoId, CancellationToken cancellationToken)
        {
            var requisito = await _context.Requisitos.FirstOrDefaultAsync(r => r.Id == requisitoId, cancellationToken);
            return requisito ?? throw new NotFoundException("el requisito", requisitoId);
        }

        private static RequisitoDto Proyectar(Requisito requisito) => new(
            requisito.Id,
            requisito.Nombre,
            requisito.Descripcion,
            requisito.EsObligatorio,
            requisito.ServicioId);
    }
}
