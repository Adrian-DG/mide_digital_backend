using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Abstraction;

namespace Domain.Entities
{
    public class Noticia : BaseEntityMetadata
    {
        public required string Titulo { get; set; }
        public required string Resumen { get; set; }
        public required string Contenido { get; set; }
        public string? ImagenUrl { get; set; }

        /// <summary>
        /// Dependencia o dirección que sindica el boletín (DIECOM / J-5).
        /// </summary>
        public required string Fuente { get; set; }

        public DateTime FechaPublicacion { get; set; }
        public bool EsDestacada { get; set; }
    }
}
