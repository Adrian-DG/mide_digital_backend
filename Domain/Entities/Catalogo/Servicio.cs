using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Abstraction;
using Domain.Common;
using Domain.Entities.Retroalimentacion;
using Domain.Entities.Tramites;

namespace Domain.Entities.Catalogo
{
    public class Servicio : BaseEntityMetadata
    {
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }
        public bool EstaDisponible { get; set; } = true;

        public int CategoriaServicioId { get; set; }
        public virtual CategoriaServicio? CategoriaServicio { get; set; }

        public int DependenciaId { get; set; }
        public virtual Dependencia? Dependencia { get; set; }

        public virtual ICollection<Requisito> Requisitos { get; set; } = new List<Requisito>();
        public virtual ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
        public virtual ICollection<CalificacionServicio> Calificaciones { get; set; } = new List<CalificacionServicio>();

        public string GenerarSecuencia()
        {
            return GenerarSecuencia(CategoriaServicio?.Nombre, Nombre);
        }

        /// <summary>
        /// Misma regla de formato para quien ya tiene los nombres sueltos
        /// (por ejemplo, el modelo de lectura) y no una instancia completa.
        /// </summary>
        public static string GenerarSecuencia(string? nombreCategoria, string? nombreServicio)
        {
            return $"SERV-{Secuencias.Fragmento(nombreCategoria)}-{Secuencias.Fragmento(nombreServicio)}";
        }
    }
}
