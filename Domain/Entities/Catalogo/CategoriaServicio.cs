using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Abstraction;

namespace Domain.Entities.Catalogo
{
    public class CategoriaServicio : BaseEntityMetadata
    {
        public required string Nombre { get; set; }
        public required string Descripcion { get; set; }

        public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
    }
}
