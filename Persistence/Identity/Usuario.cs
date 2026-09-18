using System;
using System.Collections.Generic;
using Domain.Abstraction;
using Domain.Entities.Retroalimentacion;
using Domain.Entities.Tramites;
using Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Persistence.Identity
{
    /// <summary>
    /// Usuario de la plataforma. Vive en esta capa y no en el dominio porque
    /// hereda de Identity, que es una decisión de infraestructura: el dominio
    /// solo conoce el <c>UsuarioId</c> al que apuntan solicitudes y
    /// calificaciones.
    /// </summary>
    /// <remarks>
    /// Identity aporta el identificador, el correo (<c>Email</c>), el hash de la
    /// contraseña (<c>PasswordHash</c>), el teléfono (<c>PhoneNumber</c>), la
    /// confirmación de correo y el control de intentos fallidos. Aquí solo se
    /// agregan los datos propios del MIDE.
    /// </remarks>
    public class Usuario : IdentityUser<Guid>, ISoftDeletable
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

        /// <summary>
        /// Navegaciones hacia el dominio. Existen solo en este extremo: las
        /// entidades de dominio no tienen navegación de vuelta al usuario, porque
        /// eso las obligaría a conocer esta clase.
        /// </summary>
        public virtual ICollection<Solicitud> Solicitudes { get; set; } = new List<Solicitud>();

        public virtual ICollection<CalificacionServicio> Calificaciones { get; set; } = new List<CalificacionServicio>();

        public string NombreCompleto => $"{Nombres} {Apellidos}";
    }
}
