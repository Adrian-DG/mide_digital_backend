using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Contenido;
using Application.Exceptions;
using Application.Services.Contenido;
using Dapper;
using Domain.Entities.Contenido;
using Microsoft.EntityFrameworkCore;
using Persistence.Common;
using Persistence.Connections;
using Persistence.Constants;
using Persistence.Context;

namespace Persistence.Services.Contenido
{
    public class NoticiaService : INoticiaService
    {
        private readonly MideDbContext _context;
        private readonly ISqlConnectionFactory _conexiones;

        public NoticiaService(MideDbContext context, ISqlConnectionFactory conexiones)
        {
            _context = context;
            _conexiones = conexiones;
        }

        public async Task<PagedResult<NoticiaListaDto>> ConsultarAsync(ConsultarNoticiasDto criterios, CancellationToken cancellationToken = default)
        {
            var filtros = $@"
                FROM    {Tablas.Noticias}
                WHERE   IsDeleted = 0
                        AND (@soloDestacadas IS NULL OR EsDestacada = @soloDestacadas)
                        AND (@patron IS NULL OR Titulo LIKE @patron OR Resumen LIKE @patron)";

            var sqlDatos = $@"
                SELECT  Id, Titulo, Resumen, ImagenUrl, Fuente, FechaPublicacion, EsDestacada
                {filtros}
                ORDER BY EsDestacada DESC, FechaPublicacion DESC
                OFFSET @offset ROWS FETCH NEXT @tamanoPagina ROWS ONLY";

            var termino = string.IsNullOrWhiteSpace(criterios.Termino) ? null : criterios.Termino.Trim();
            var parametros = new
            {
                patron = termino is null ? null : $"%{Sql.EscaparLike(termino)}%",
                soloDestacadas = criterios.SoloDestacadas,
                offset = criterios.Offset,
                tamanoPagina = criterios.TamanoPagina
            };

            using var conexion = _conexiones.CrearConexion();

            var items = await conexion.QueryAsync<NoticiaListaDto>(
                new CommandDefinition(sqlDatos, parametros, cancellationToken: cancellationToken));

            var total = await conexion.ExecuteScalarAsync<int>(
                new CommandDefinition($"SELECT COUNT(1){filtros}", parametros, cancellationToken: cancellationToken));

            return new PagedResult<NoticiaListaDto>
            {
                Items = items.ToList(),
                TotalRegistros = total,
                Pagina = criterios.Pagina,
                TamanoPagina = criterios.TamanoPagina
            };
        }

        public async Task<NoticiaDetalleDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var sql = $@"
                SELECT  Id, Titulo, Resumen, Contenido, ImagenUrl, Fuente, FechaPublicacion, EsDestacada
                FROM    {Tablas.Noticias}
                WHERE   Id = @id AND IsDeleted = 0";

            using var conexion = _conexiones.CrearConexion();
            var noticia = await conexion.QuerySingleOrDefaultAsync<NoticiaDetalleDto>(
                new CommandDefinition(sql, new { id }, cancellationToken: cancellationToken));

            return noticia ?? throw new NotFoundException("la noticia", id);
        }

        public async Task<NoticiaDetalleDto> CrearAsync(GuardarNoticiaDto dto, CancellationToken cancellationToken = default)
        {
            var noticia = new Noticia
            {
                Titulo = dto.Titulo.Trim(),
                Resumen = dto.Resumen.Trim(),
                Contenido = dto.Contenido,
                ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim(),
                Fuente = dto.Fuente.Trim(),
                FechaPublicacion = dto.FechaPublicacion,
                EsDestacada = dto.EsDestacada
            };

            _context.Noticias.Add(noticia);
            await _context.SaveChangesAsync(cancellationToken);

            return Proyectar(noticia);
        }

        public async Task<NoticiaDetalleDto> ActualizarAsync(int id, GuardarNoticiaDto dto, CancellationToken cancellationToken = default)
        {
            var noticia = await ObtenerParaEscrituraAsync(id, cancellationToken);

            noticia.Titulo = dto.Titulo.Trim();
            noticia.Resumen = dto.Resumen.Trim();
            noticia.Contenido = dto.Contenido;
            noticia.ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim();
            noticia.Fuente = dto.Fuente.Trim();
            noticia.FechaPublicacion = dto.FechaPublicacion;
            noticia.EsDestacada = dto.EsDestacada;

            await _context.SaveChangesAsync(cancellationToken);

            return Proyectar(noticia);
        }

        public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
        {
            var noticia = await ObtenerParaEscrituraAsync(id, cancellationToken);

            noticia.IsDeleted = true;
            await _context.SaveChangesAsync(cancellationToken);
        }

        private async Task<Noticia> ObtenerParaEscrituraAsync(int id, CancellationToken cancellationToken)
        {
            var noticia = await _context.Noticias.FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
            return noticia ?? throw new NotFoundException("la noticia", id);
        }

        private static NoticiaDetalleDto Proyectar(Noticia noticia) => new(
            noticia.Id,
            noticia.Titulo,
            noticia.Resumen,
            noticia.Contenido,
            noticia.ImagenUrl,
            noticia.Fuente,
            noticia.FechaPublicacion,
            noticia.EsDestacada);
    }
}
