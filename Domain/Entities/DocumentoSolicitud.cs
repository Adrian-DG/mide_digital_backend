using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Abstraction;

namespace Domain.Entities
{
    public class DocumentoSolicitud : BaseEntityMetadata
    {
        public required string NombreArchivo { get; set; }
        public required string RutaArchivo { get; set; }
        public required string TipoContenido { get; set; }
        public long TamanoBytes { get; set; }
        public DateTime FechaCarga { get; set; }

        public int SolicitudId { get; set; }
        public virtual Solicitud? Solicitud { get; set; }

        /// <summary>
        /// Requisito que este archivo satisface, para que el revisor sepa
        /// qué documento corresponde a qué condición del trámite.
        /// </summary>
        public int RequisitoId { get; set; }
        public virtual Requisito? Requisito { get; set; }
    }
}
