using Domain.Common;
using Domain.Entities.Catalogo;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations.Catalogo
{
    public class DependenciaConfiguration : IEntityTypeConfiguration<Dependencia>
    {
        public void Configure(EntityTypeBuilder<Dependencia> builder)
        {
            builder.ToTable("Dependencias", Esquemas.Catalogo);

            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).ValueGeneratedOnAdd();

            builder.Property(d => d.Nombre).HasMaxLength(200).IsRequired();
            builder.Property(d => d.Siglas).HasMaxLength(20).IsRequired();
            builder.Property(d => d.Institucion).HasConversion<int>().IsRequired();
            builder.Property(d => d.AreaFuncional).HasConversion<int>().IsRequired();

            builder.HasIndex(d => d.Siglas).IsUnique().HasDatabaseName("UX_Dependencias_Siglas");
            builder.HasIndex(d => d.AreaFuncional).HasDatabaseName("IX_Dependencias_AreaFuncional");

            builder.HasMany(d => d.Servicios)
                   .WithOne(s => s.Dependencia)
                   .HasForeignKey(s => s.DependenciaId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
