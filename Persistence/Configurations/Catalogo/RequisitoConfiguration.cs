using Domain.Common;
using Domain.Entities.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations.Catalogo
{
    public class RequisitoConfiguration : IEntityTypeConfiguration<Requisito>
    {
        public void Configure(EntityTypeBuilder<Requisito> builder)
        {
            builder.ToTable("Requisitos", Esquemas.Catalogo);

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedOnAdd();

            builder.Property(r => r.Nombre).HasMaxLength(200).IsRequired();
            builder.Property(r => r.Descripcion).HasMaxLength(1000).IsRequired();
            builder.Property(r => r.EsObligatorio).IsRequired();

            builder.HasIndex(r => new { r.ServicioId, r.EsObligatorio })
                   .HasDatabaseName("IX_Requisitos_Servicio_Obligatorio");
        }
    }
}
