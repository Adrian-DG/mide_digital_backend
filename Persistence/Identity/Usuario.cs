using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Abstraction;
using Domain.Entities.Retroalimentacion;
using Domain.Entities.Tramites;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identidad
{
    /// <summary>
    /// Usuario de la plataforma. Hereda de Identity, que aporta el identificador,
    /// el correo (<c>Email</c>), el hash de la contraseña (<c>PasswordHash</c>),
    /// el teléfono (<c>PhoneNumber</c>), la confirmación de correo y el control de
    /// intentos fallidos. Aquí solo viven los datos propios del MIDE.
    /// </summary>
    public class Usuario : IdentityUser<int>, ISoftDeletable
    {
        public required string Nombres { get; set; }
        public required string Apellidos { get; set; }

        /// <summary>
        /// Clasificación funcional del perfil (militar activo o retirado, familiar,
        /// ciudadano). No es un permiso: determina qué oferta de servicios aplica.
        /// </summary>
        public TipoUsuario TipoUsuario { get; set; }

        public string? Cedula { get; set; }
        public DateTime FechaRegistro { get; set; }

        public bool IsDeleted { get; set; }

        public virtual ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();
        public virtual ICollection<CalificacionServicio> Calificaciones { get; set; } = new List<CalificacionServicio>();

        public string NombreCompleto => $"{Nombres} {Apellidos}";
    }
}
