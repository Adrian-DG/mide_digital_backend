using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Abstraction;
using Domain.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Persistence.Interceptors
{
    /// <summary>
    /// Sella las cuatro columnas de auditoría de toda entidad que implemente
    /// <see cref="IAuditableEntityMetadata"/>, justo antes de que EF envíe los
    /// cambios. Vive en un interceptor y no en el contexto para que la auditoría
    /// no dependa de que cada servicio se acuerde de asignarla, y para que el
    /// contexto no tenga que conocer al usuario actual ni al reloj.
    /// </summary>
    public class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        /// <summary>
        /// Valor que se considera "sin asignar" en CreatedBy y UpdatedBy. Si el
        /// servicio ya puso un identificador explícito (por ejemplo, el revisor
        /// que aprueba un trámite en nombre de otro), el interceptor lo respeta.
        /// </summary>
        private const int SinAsignar = 0;

        private readonly IUsuarioActual _usuarioActual;
        private readonly TimeProvider _reloj;

        public AuditableEntityInterceptor(IUsuarioActual usuarioActual, TimeProvider reloj)
        {
            _usuarioActual = usuarioActual;
            _reloj = reloj;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            Sellar(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            Sellar(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void Sellar(DbContext? context)
        {
            if (context is null)
            {
                return;
            }

            var ahora = _reloj.GetUtcNow().UtcDateTime;
            var usuarioId = _usuarioActual.UsuarioId;

            foreach (var entrada in context.ChangeTracker.Entries<IAuditableEntityMetadata>())
            {
                switch (entrada.State)
                {
                    case EntityState.Added:
                        entrada.Entity.CreatedAt = ahora;
                        entrada.Entity.UpdatedAt = ahora;

                        if (entrada.Entity.CreatedBy == SinAsignar && usuarioId.HasValue)
                        {
                            entrada.Entity.CreatedBy = usuarioId.Value;
                        }

                        if (entrada.Entity.UpdatedBy == SinAsignar)
                        {
                            entrada.Entity.UpdatedBy = entrada.Entity.CreatedBy;
                        }

                        break;

                    case EntityState.Modified:
                        entrada.Entity.UpdatedAt = ahora;

                        if (usuarioId.HasValue && !FueAsignadoEnEstaOperacion(entrada))
                        {
                            entrada.Entity.UpdatedBy = usuarioId.Value;
                        }

                        // Quién y cuándo creó el registro no se reescribe nunca,
                        // aunque la entidad venga con otros valores en memoria.
                        entrada.Property(nameof(IAuditableEntityMetadata.CreatedAt)).IsModified = false;
                        entrada.Property(nameof(IAuditableEntityMetadata.CreatedBy)).IsModified = false;
                        break;
                }
            }
        }

        /// <summary>
        /// True si el servicio ya cambió UpdatedBy a propósito en esta operación,
        /// caso en el que su valor manda sobre el usuario de la petición.
        /// </summary>
        private static bool FueAsignadoEnEstaOperacion(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<IAuditableEntityMetadata> entrada)
        {
            var propiedad = entrada.Property(nameof(IAuditableEntityMetadata.UpdatedBy));

            return propiedad.IsModified
                   && propiedad.CurrentValue is int actual
                   && actual != SinAsignar;
        }
    }
}
