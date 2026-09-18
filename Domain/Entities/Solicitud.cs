using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Abstraction;
using Domain.Enums;

namespace Domain.Entities
{
    public class Solicitud : BaseEntityMetadata, IAuditableEntityMetadata
    {
        public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.PENDIENTE;
        public string? Observaciones { get; set; }
        public DateTime? FechaProcesamiento { get; set; }

        public int ServicioId { get; set; }
        public virtual Servicio? Servicio { get; set; }

        public int UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }

        public virtual ICollection<DocumentoSolicitud> Documentos { get; set; } = new List<DocumentoSolicitud>();

        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UpdatedBy { get; set; }

        public string GenerarSecuencia()
        {
            return $"SOL-{Servicio?.GenerarSecuencia()}-{Id}";
        }
    }
}
