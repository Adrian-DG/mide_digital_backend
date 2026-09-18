using System;
using System.Collections.Generic;

namespace Application.Common
{
    /// <summary>
    /// Página de resultados para los listados del directorio y del buscador.
    /// </summary>
    public class PagedResult<T>
    {
        public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();
        public int TotalRegistros { get; init; }
        public int Pagina { get; init; }
        public int TamanoPagina { get; init; }

        public int TotalPaginas => TamanoPagina <= 0
            ? 0
            : (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);

        public bool TienePaginaSiguiente => Pagina < TotalPaginas;
    }
}
