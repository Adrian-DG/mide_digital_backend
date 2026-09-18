using Domain.Common;
using Domain.Entities.Tramites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Identity;

namespace Persistence.Configurations.Tramites
{
    public class SolicitudConfiguration : IEntityTypeConfiguration<Solicitud>
    {
        public void Configure(EntityTypeBuilder<Solicitud> builder)
        {
            builder.ToTable("Solicitudes", Esquemas.Tramites);

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).ValueGeneratedOnAdd();

            builder.Property(s => s.Estado).HasConversion<int>().IsRequired();
            builder.Property(s => s.Observaciones).HasMaxLength(2000);
            builder.Property(s => s.CreatedAt).IsRequired();
            builder.Property(s => s.UpdatedAt).IsRequired();

            builder.HasIndex(s => new { s.UsuarioId, s.Estado })
                   .HasDatabaseName("IX_Solicitudes_Usuario_Estado");
            builder.HasIndex(s => new { s.ServicioId, s.Estado })
                   .HasDatabaseName("IX_Solicitudes_Servicio_Estado");
            builder.HasIndex(s => s.CreatedAt).HasDatabaseName("IX_Solicitudes_CreatedAt");

            // Relación configurada solo desde este extremo: Solicitud no tiene
            // navegación al usuario, que es una entidad de esta capa.
            builder.HasOne<Usuario>()
                   .WithMany(u => u.Solicitudes)
                   .HasForeignKey(s => s.UsuarioId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Documentos)
                   .WithOne(d => d.Solicitud)
                   .HasForeignKey(d => d.SolicitudId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
