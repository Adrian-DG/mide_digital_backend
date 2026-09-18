using Domain.Common;
using Domain.Entities.Retroalimentacion;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Identity;

namespace Persistence.Configurations.Retroalimentacion
{
    public class CalificacionServicioConfiguration : IEntityTypeConfiguration<CalificacionServicio>
    {
        public void Configure(EntityTypeBuilder<CalificacionServicio> builder)
        {
            builder.ToTable(
                "CalificacionesServicio",
                Esquemas.Retroalimentacion,
                tabla => tabla.HasCheckConstraint(
                    "CK_CalificacionesServicio_Estrellas",
                    $"[Estrellas] BETWEEN {CalificacionServicio.EstrellasMinimas} AND {CalificacionServicio.EstrellasMaximas}"));

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedOnAdd();

            // El setter de Estrellas valida el rango 1-5 y lanza si se sale de él.
            // EF debe escribir el campo de respaldo para no reventar al materializar
            // filas históricas que no cumplan la regla.
            builder.Property(c => c.Estrellas)
                   .HasField("_estrellas")
                   .UsePropertyAccessMode(PropertyAccessMode.Field)
                   .IsRequired();

            builder.Property(c => c.Comentario).HasMaxLength(2000);
            builder.Property(c => c.FechaCalificacion).IsRequired();

            builder.HasIndex(c => new { c.ServicioId, c.UsuarioId })
                   .HasDatabaseName("IX_CalificacionesServicio_Servicio_Usuario");

            // Ver la nota en SolicitudConfiguration: la navegación existe solo
            // en el extremo del usuario.
            builder.HasOne<Usuario>()
                   .WithMany(u => u.Calificaciones)
                   .HasForeignKey(c => c.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
