using Domain.Common;
using Domain.Entities.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations.Catalogo
{
    public class CategoriaServicioConfiguration : IEntityTypeConfiguration<CategoriaServicio>
    {
        public void Configure(EntityTypeBuilder<CategoriaServicio> builder)
        {
            builder.ToTable("CategoriasServicio", Esquemas.Catalogo);

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedOnAdd();

            builder.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            builder.Property(c => c.Descripcion).HasMaxLength(500).IsRequired();

            builder.HasIndex(c => c.Nombre).IsUnique().HasDatabaseName("UX_CategoriasServicio_Nombre");

            builder.HasMany(c => c.Servicios)
                   .WithOne(s => s.CategoriaServicio)
                   .HasForeignKey(s => s.CategoriaServicioId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
