using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Abstraction;

namespace Domain.Entities
{
    public class CalificacionServicio : BaseEntityMetadata
    {
        public const int EstrellasMinimas = 1;
        public const int EstrellasMaximas = 5;

        private int _estrellas;

        /// <summary>
        /// Escala de 1 a 5 estrellas. Fuera de ese rango la calificación no es válida.
        /// </summary>
        public int Estrellas
        {
            get => _estrellas;
            set
            {
                if (value < EstrellasMinimas || value > EstrellasMaximas)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(Estrellas),
                        value,
                        $"La calificación debe estar entre {EstrellasMinimas} y {EstrellasMaximas} estrellas.");
                }

                _estrellas = value;
            }
        }

        public string? Comentario { get; set; }
        public DateTime FechaCalificacion { get; set; }

        public int UsuarioId { get; set; }
        public virtual Usuario? Usuario { get; set; }

        public int ServicioId { get; set; }
        public virtual Servicio? Servicio { get; set; }
    }
}
