using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Abstraction;
using Domain.Entities.Retroalimentacion;
using Domain.Entities.Tramites;
using Domain.Enums;

namespace Domain.Entities.Identidad
{
    public class Usuario : BaseEntityMetadata
    {
        public required string Nombres { get; set; }
        public required string Apellidos { get; set; }
        public required string Correo { get; set; }

        /// <summary>
        /// Hash de la contraseña. El dominio nunca almacena la contraseña en claro.
        /// </summary>
        public required string ContrasenaHash { get; set; }

        public TipoUsuario TipoUsuario { get; set; }
        public string? Cedula { get; set; }
        public string? Telefono { get; set; }
        public bool CorreoConfirmado { get; set; }
        public DateTime FechaRegistro { get; set; }

        public virtual ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
        public virtual ICollection<CalificacionServicio> Calificaciones { get; set; } = new List<CalificacionServicio>();

        public string NombreCompleto => $"{Nombres} {Apellidos}";
    }
}
