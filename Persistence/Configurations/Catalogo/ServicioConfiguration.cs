using Domain.Common;
using Domain.Entities.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations.Catalogo
{
    public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
    {
        public void Configure(EntityTypeBuilder<Servicio> builder)
        {
            builder.ToTable("Servicios", Esquemas.Catalogo);

            builder.HasKey(s => s.Id);
            builder.Property(s => s.Id).ValueGeneratedOnAdd();

            builder.Property(s => s.Nombre).HasMaxLength(200).IsRequired();
            builder.Property(s => s.Descripcion).HasMaxLength(1000).IsRequired();
            builder.Property(s => s.EstaDisponible).IsRequired();

            builder.HasIndex(s => s.Nombre).HasDatabaseName("IX_Servicios_Nombre");
            builder.HasIndex(s => new { s.CategoriaServicioId, s.EstaDisponible })
                   .HasDatabaseName("IX_Servicios_Categoria_Disponible");

            builder.HasMany(s => s.Requisitos)
                   .WithOne(r => r.Servicio)
                   .HasForeignKey(r => r.ServicioId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Solicitudes)
                   .WithOne(so => so.Servicio)
                   .HasForeignKey(so => so.ServicioId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Calificaciones)
                   .WithOne(c => c.Servicio)
                   .HasForeignKey(c => c.ServicioId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
