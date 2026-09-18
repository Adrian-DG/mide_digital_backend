using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Abstraction;
using Domain.Entities.Catalogo;
using Domain.Entities.Identidad;
using Domain.Enums;

namespace Domain.Entities.Tramites
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
            return GenerarSecuencia(Servicio?.CategoriaServicio?.Nombre, Servicio?.Nombre, Id);
        }

        /// <summary>
        /// Misma regla de formato para quien ya tiene los nombres sueltos
        /// (por ejemplo, el modelo de lectura) y no una instancia completa.
        /// </summary>
        public static string GenerarSecuencia(string? nombreCategoria, string? nombreServicio, int id)
        {
            return $"SOL-{Servicio.GenerarSecuencia(nombreCategoria, nombreServicio)}-{id}";
        }
    }
}
