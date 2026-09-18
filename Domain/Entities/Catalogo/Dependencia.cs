using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Abstraction;
using Domain.Enums;

namespace Domain.Entities.Catalogo
{
    public class Dependencia : BaseEntityMetadata
    {
        public required string Nombre { get; set; }
        public required string Siglas { get; set; }
        public Institucion Institucion { get; set; }
        public AreaFuncional AreaFuncional { get; set; }

        public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
    }
}
