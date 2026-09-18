using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Abstraction;
using Domain.Entities.Tramites;

namespace Domain.Entities.Catalogo
{
    public class Requisito : BaseEntityMetadata
    {
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public bool EsObligatorio { get; set; }

        public int ServicioId { get; set; }
        public virtual Servicio? Servicio { get; set; }

        public virtual ICollection<DocumentoSolicitud> Documentos { get; set; } = new List<DocumentoSolicitud>();
    }
}
