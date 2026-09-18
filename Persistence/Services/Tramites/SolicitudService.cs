using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Tramites;
using Application.Exceptions;
using Application.Services.Tramites;
using Dapper;
using Domain.Entities.Catalogo;
using Domain.Entities.Tramites;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Persistence.Connections;
using Persistence.Constants;
using Persistence.Context;

namespace Persistence.Services.Tramites
{
    public class SolicitudService : ISolicitudService
    {
        /// <summary>
        /// Transiciones permitidas del trámite. Un estado terminal no vuelve atrás:
        /// reabrir un trámite rechazado significa crear uno nuevo.
        /// </summary>
        private static readonly Dictionary<EstadoSolicitud, EstadoSolicitud[]> TransicionesValidas = new()
        {
            [EstadoSolicitud.PENDIENTE] = new[] { EstadoSolicitud.EN_PROCESO, EstadoSolicitud.RECHAZADA },
            [EstadoSolicitud.EN_PROCESO] = new[] { EstadoSolicitud.APROBADA, EstadoSolicitud.RECHAZADA },
            [EstadoSolicitud.APROBADA] = Array.Empty<EstadoSolicitud>(),
            [EstadoSolicitud.RECHAZADA] = Array.Empty<EstadoSolicitud>()
        };

        private readonly MideDbContext _context;
        private readonly ISqlConnectionFactory _conexiones;

        public SolicitudService(MideDbContext context, ISqlConnectionFactory conexiones)
        {
            _context = context;
            _conexiones = conexiones;
        }

        public async Task<PagedResult<SolicitudListaDto>> ConsultarAsync(ConsultarSolicitudesDto criterios, CancellationToken cancellationToken = default)
        {
            var origen = $@"
                FROM    {Tablas.Solicitudes} sol
                INNER JOIN {Tablas.Servicios} s ON s.Id = sol.ServicioId
                INNER JOIN {Tablas.CategoriasServicio} c ON c.Id = s.CategoriaServicioId
                INNER JOIN {Tablas.Dependencias} d ON d.Id = s.DependenciaId
                INNER JOIN {Tablas.Usuarios} u ON u.Id = sol.UsuarioId
                WHERE   sol.IsDeleted = 0
                        AND (@usuarioId IS NULL OR sol.UsuarioId = @usuarioId)
                        AND (@servicioId IS NULL OR sol.ServicioId = @servicioId)
                        AND (@dependenciaId IS NULL OR s.DependenciaId = @dependenciaId)
                        AND (@estado IS NULL OR sol.Estado = @estado)
                        AND (@desde IS NULL OR sol.CreatedAt >= @desde)
                        AND (@hasta IS NULL OR sol.CreatedAt < @hasta)";

            var sqlDatos = $@"
                SELECT  sol.Id,
                        sol.Estado,
                        sol.CreatedAt,
                        sol.FechaProcesamiento,
                        sol.ServicioId,
                        s.Nombre AS ServicioNombre,
                        c.Nombre AS CategoriaNombre,
                        d.Siglas AS DependenciaSiglas,
                        sol.UsuarioId,
                        u.Nombres + ' ' + u.Apellidos AS UsuarioNombre,
                        (SELECT COUNT(1) FROM {Tablas.DocumentosSolicitud} doc
                         WHERE doc.SolicitudId = sol.Id AND doc.IsDeleted = 0) AS TotalDocumentos
                {origen}
                ORDER BY sol.CreatedAt DESC
                OFFSET @offset ROWS FETCH NEXT @tamanoPagina ROWS ONLY";

            var parametros = new
            {
                usuarioId = criterios.UsuarioId,
                servicioId = criterios.ServicioId,
                dependenciaId = criterios.DependenciaId,
                estado = (int?)criterios.Estado,
                desde = criterios.Desde,
                hasta = criterios.Hasta,
                offset = criterios.Offset,
                tamanoPagina = criterios.TamanoPagina
            };

            using var conexion = _conexiones.CrearConexion();

            var filas = await conexion.QueryAsync<FilaSolicitud>(
                new CommandDefinition(sqlDatos, parametros, cancellationToken: cancellationToken));

            var total = await conexion.ExecuteScalarAsync<int>(
                new CommandDefinition($"SELECT COUNT(1){origen}", parametros, cancellationToken: cancellationToken));

            return new PagedResult<SolicitudListaDto>
            {
                Items = filas.Select(f => f.ADto()).ToList(),
                TotalRegistros = total,
                Pagina = criterios.Pagina,
                TamanoPagina = criterios.TamanoPagina
            };
        }

        public async Task<SolicitudDetalleDto> ObtenerDetalleAsync(int id, CancellationToken cancellationToken = default)
        {
            var sqlSolicitud = $@"
                SELECT  sol.Id,
                        sol.Estado,
                        sol.Observaciones,
                        sol.CreatedAt,
                        sol.FechaProcesamiento,
                        sol.ServicioId,
                        s.Nombre AS ServicioNombre,
                        c.Nombre AS CategoriaNombre,
                        s.DependenciaId,
                        d.Nombre AS DependenciaNombre,
                        sol.UsuarioId,
                        u.Nombres + ' ' + u.Apellidos AS UsuarioNombre,
                        u.Email AS UsuarioCorreo
                FROM    {Tablas.Solicitudes} sol
                INNER JOIN {Tablas.Servicios} s ON s.Id = sol.ServicioId
                INNER JOIN {Tablas.CategoriasServicio} c ON c.Id = s.CategoriaServicioId
                INNER JOIN {Tablas.Dependencias} d ON d.Id = s.DependenciaId
                INNER JOIN {Tablas.Usuarios} u ON u.Id = sol.UsuarioId
                WHERE   sol.Id = @id AND sol.IsDeleted = 0";

            var sqlDocumentos = $@"
                SELECT  doc.Id,
                        doc.NombreArchivo,
                        doc.RutaArchivo,
                        doc.TipoContenido,
                        doc.TamanoBytes,
                        doc.FechaCarga,
                        doc.SolicitudId,
                        doc.RequisitoId,
                        r.Nombre AS RequisitoNombre,
                        r.EsObligatorio AS RequisitoEsObligatorio
                FROM    {Tablas.DocumentosSolicitud} doc
                INNER JOIN {Tablas.Requisitos} r ON r.Id = doc.RequisitoId
                WHERE   doc.SolicitudId = @id AND doc.IsDeleted = 0
                ORDER BY r.EsObligatorio DESC, doc.FechaCarga";

            using var conexion = _conexiones.CrearConexion();

            var fila = await conexion.QuerySingleOrDefaultAsync<FilaDetalle>(
                new CommandDefinition(sqlSolicitud, new { id }, cancellationToken: cancellationToken));

            if (fila is null)
            {
                throw new NotFoundException("la solicitud", id);
            }

            var documentos = await conexion.QueryAsync<DocumentoSolicitudDto>(
                new CommandDefinition(sqlDocumentos, new { id }, cancellationToken: cancellationToken));

            return fila.ADto(documentos.ToList());
        }

        public async Task<SolicitudDetalleDto> CrearAsync(CrearSolicitudDto dto, CancellationToken cancellationToken = default)
        {
            var servicio = await _context.Servicios
                .FirstOrDefaultAsync(s => s.Id == dto.ServicioId, cancellationToken)
                ?? throw new NotFoundException("el servicio", dto.ServicioId);

            if (!servicio.EstaDisponible)
            {
                throw new BusinessRuleException($"El servicio {servicio.Nombre} no está disponible para solicitudes.");
            }

            var existeUsuario = await _context.Usuarios.AnyAsync(u => u.Id == dto.UsuarioId, cancellationToken);
            if (!existeUsuario)
            {
                throw new NotFoundException("el usuario", dto.UsuarioId);
            }

            var yaTieneAbierta = await _context.Solicitudes.AnyAsync(
                s => s.UsuarioId == dto.UsuarioId
                     && s.ServicioId == dto.ServicioId
                     && (s.Estado == EstadoSolicitud.PENDIENTE || s.Estado == EstadoSolicitud.EN_PROCESO),
                cancellationToken);

            if (yaTieneAbierta)
            {
                throw new BusinessRuleException(
                    $"Ya existe un trámite en curso de este usuario para {servicio.Nombre}.");
            }

            var solicitud = new Solicitud
            {
                ServicioId = dto.ServicioId,
                UsuarioId = dto.UsuarioId,
                Estado = EstadoSolicitud.PENDIENTE,
                Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim()

                // CreatedBy y UpdatedBy los sella el interceptor con el usuario
                // de la petición: UsuarioId es el solicitante, CreatedBy es quien
                // registró el trámite, que no siempre son la misma persona.
            };

            _context.Solicitudes.Add(solicitud);
            await _context.SaveChangesAsync(cancellationToken);

            return await ObtenerDetalleAsync(solicitud.Id, cancellationToken);
        }

        public async Task<DocumentoSolicitudDto> AdjuntarDocumentoAsync(int solicitudId, AdjuntarDocumentoDto dto, CancellationToken cancellationToken = default)
        {
            var solicitud = await ObtenerParaEscrituraAsync(solicitudId, cancellationToken);

            if (solicitud.Estado is EstadoSolicitud.APROBADA or EstadoSolicitud.RECHAZADA)
            {
                throw new BusinessRuleException("No se pueden adjuntar documentos a un trámite ya cerrado.");
            }

            var requisito = await _context.Requisitos
                .FirstOrDefaultAsync(r => r.Id == dto.RequisitoId, cancellationToken)
                ?? throw new NotFoundException("el requisito", dto.RequisitoId);

            if (requisito.ServicioId != solicitud.ServicioId)
            {
                throw new BusinessRuleException(
                    "El requisito no pertenece al servicio que se está tramitando.");
            }

            var documento = new DocumentoSolicitud
            {
                SolicitudId = solicitudId,
                RequisitoId = dto.RequisitoId,
                NombreArchivo = dto.NombreArchivo.Trim(),
                RutaArchivo = dto.RutaArchivo.Trim(),
                TipoContenido = dto.TipoContenido.Trim(),
                TamanoBytes = dto.TamanoBytes,
                FechaCarga = DateTime.UtcNow
            };

            _context.DocumentosSolicitud.Add(documento);
            await _context.SaveChangesAsync(cancellationToken);

            return new DocumentoSolicitudDto(
                documento.Id,
                documento.NombreArchivo,
                documento.RutaArchivo,
                documento.TipoContenido,
                documento.TamanoBytes,
                documento.FechaCarga,
                documento.SolicitudId,
                documento.RequisitoId,
                requisito.Nombre,
                requisito.EsObligatorio);
        }

        public async Task EliminarDocumentoAsync(int solicitudId, int documentoId, CancellationToken cancellationToken = default)
        {
            var solicitud = await ObtenerParaEscrituraAsync(solicitudId, cancellationToken);

            if (solicitud.Estado is EstadoSolicitud.APROBADA or EstadoSolicitud.RECHAZADA)
            {
                throw new BusinessRuleException("No se pueden retirar documentos de un trámite ya cerrado.");
            }

            var documento = await _context.DocumentosSolicitud
                .FirstOrDefaultAsync(d => d.Id == documentoId && d.SolicitudId == solicitudId, cancellationToken)
                ?? throw new NotFoundException("el documento", documentoId);

            documento.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<SolicitudDetalleDto> CambiarEstadoAsync(
            int solicitudId,
            CambiarEstadoSolicitudDto dto,
            int usuarioResponsableId,
            CancellationToken cancellationToken = default)
        {
            var solicitud = await ObtenerParaEscrituraAsync(solicitudId, cancellationToken);

            if (!TransicionesValidas.TryGetValue(solicitud.Estado, out var permitidos) || !permitidos.Contains(dto.NuevoEstado))
            {
                throw new BusinessRuleException(
                    $"No se puede pasar de {solicitud.Estado} a {dto.NuevoEstado}.");
            }

            if (dto.NuevoEstado == EstadoSolicitud.APROBADA)
            {
                var pendientes = await LeerRequisitosPendientesAsync(solicitudId, cancellationToken);
                if (pendientes.Count > 0)
                {
                    throw new BusinessRuleException(
                        $"Faltan requisitos obligatorios sin documento: {string.Join(", ", pendientes)}.");
                }
            }

            if (dto.NuevoEstado == EstadoSolicitud.RECHAZADA && string.IsNullOrWhiteSpace(dto.Observaciones))
            {
                throw new BusinessRuleException("Un rechazo debe indicar el motivo en las observaciones.");
            }

            solicitud.Estado = dto.NuevoEstado;
            solicitud.UpdatedBy = usuarioResponsableId;

            if (!string.IsNullOrWhiteSpace(dto.Observaciones))
            {
                solicitud.Observaciones = dto.Observaciones.Trim();
            }

            if (dto.NuevoEstado is EstadoSolicitud.APROBADA or EstadoSolicitud.RECHAZADA)
            {
                solicitud.FechaProcesamiento = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return await ObtenerDetalleAsync(solicitudId, cancellationToken);
        }

        public async Task<IReadOnlyCollection<string>> ListarRequisitosPendientesAsync(int solicitudId, CancellationToken cancellationToken = default)
        {
            var existe = await _context.Solicitudes.AnyAsync(s => s.Id == solicitudId, cancellationToken);
            if (!existe)
            {
                throw new NotFoundException("la solicitud", solicitudId);
            }

            return await LeerRequisitosPendientesAsync(solicitudId, cancellationToken);
        }

        public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
        {
            var solicitud = await ObtenerParaEscrituraAsync(id, cancellationToken);

            solicitud.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<IReadOnlyCollection<string>> LeerRequisitosPendientesAsync(int solicitudId, CancellationToken cancellationToken)
        {
            var sql = $@"
                SELECT  r.Nombre
                FROM    {Tablas.Solicitudes} sol
                INNER JOIN {Tablas.Requisitos} r ON r.ServicioId = sol.ServicioId AND r.IsDeleted = 0
                WHERE   sol.Id = @solicitudId
                        AND r.EsObligatorio = 1
                        AND NOT EXISTS (SELECT 1
                                        FROM   {Tablas.DocumentosSolicitud} doc
                                        WHERE  doc.SolicitudId = sol.Id
                                               AND doc.RequisitoId = r.Id
                                               AND doc.IsDeleted = 0)
                ORDER BY r.Nombre";

            using var conexion = _conexiones.CrearConexion();
            var pendientes = await conexion.QueryAsync<string>(
                new CommandDefinition(sql, new { solicitudId }, cancellationToken: cancellationToken));

            return pendientes.ToList();
        }

        private async Task<Solicitud> ObtenerParaEscrituraAsync(int id, CancellationToken cancellationToken)
        {
            var solicitud = await _context.Solicitudes.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            return solicitud ?? throw new NotFoundException("la solicitud", id);
        }

        private sealed class FilaSolicitud
        {
            public int Id { get; init; }
            public EstadoSolicitud Estado { get; init; }
            public DateTime CreatedAt { get; init; }
            public DateTime? FechaProcesamiento { get; init; }
            public int ServicioId { get; init; }
            public string ServicioNombre { get; init; } = string.Empty;
            public string CategoriaNombre { get; init; } = string.Empty;
            public string DependenciaSiglas { get; init; } = string.Empty;
            public int UsuarioId { get; init; }
            public string UsuarioNombre { get; init; } = string.Empty;
            public int TotalDocumentos { get; init; }

            public SolicitudListaDto ADto() => new(
                Id,
                Secuencia(Id, CategoriaNombre, ServicioNombre),
                Estado,
                CreatedAt,
                FechaProcesamiento,
                ServicioId,
                ServicioNombre,
                DependenciaSiglas,
                UsuarioId,
                UsuarioNombre,
                TotalDocumentos);
        }

        private sealed class FilaDetalle
        {
            public int Id { get; init; }
            public EstadoSolicitud Estado { get; init; }
            public string? Observaciones { get; init; }
            public DateTime CreatedAt { get; init; }
            public DateTime? FechaProcesamiento { get; init; }
            public int ServicioId { get; init; }
            public string ServicioNombre { get; init; } = string.Empty;
            public string CategoriaNombre { get; init; } = string.Empty;
            public int DependenciaId { get; init; }
            public string DependenciaNombre { get; init; } = string.Empty;
            public int UsuarioId { get; init; }
            public string UsuarioNombre { get; init; } = string.Empty;
            public string UsuarioCorreo { get; init; } = string.Empty;

            public SolicitudDetalleDto ADto(IReadOnlyCollection<DocumentoSolicitudDto> documentos) => new()
            {
                Id = Id,
                Secuencia = Secuencia(Id, CategoriaNombre, ServicioNombre),
                Estado = Estado,
                Observaciones = Observaciones,
                CreatedAt = CreatedAt,
                FechaProcesamiento = FechaProcesamiento,
                ServicioId = ServicioId,
                ServicioNombre = ServicioNombre,
                DependenciaId = DependenciaId,
                DependenciaNombre = DependenciaNombre,
                UsuarioId = UsuarioId,
                UsuarioNombre = UsuarioNombre,
                UsuarioCorreo = UsuarioCorreo,
                Documentos = documentos
            };
        }

        /// <summary>
        /// Reconstruye el código visible del trámite con la regla del dominio.
        /// </summary>
        private static string Secuencia(int id, string categoria, string servicio)
            => Solicitud.GenerarSecuencia(categoria, servicio, id);
    }
}
