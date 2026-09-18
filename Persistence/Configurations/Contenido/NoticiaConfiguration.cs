using Domain.Common;
using Domain.Entities.Contenido;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations.Contenido
{
    public class NoticiaConfiguration : IEntityTypeConfiguration<Noticia>
    {
        public void Configure(EntityTypeBuilder<Noticia> builder)
        {
            builder.ToTable("Noticias", Esquemas.Contenido);

            builder.HasKey(n => n.Id);
            builder.Property(n => n.Id).ValueGeneratedOnAdd();

            builder.Property(n => n.Titulo).HasMaxLength(300).IsRequired();
            builder.Property(n => n.Resumen).HasMaxLength(1000).IsRequired();
            builder.Property(n => n.Contenido).IsRequired();
            builder.Property(n => n.ImagenUrl).HasMaxLength(1000);
            builder.Property(n => n.Fuente).HasMaxLength(150).IsRequired();
            builder.Property(n => n.FechaPublicacion).IsRequired();

            builder.HasIndex(n => n.FechaPublicacion).HasDatabaseName("IX_Noticias_FechaPublicacion");
            builder.HasIndex(n => n.EsDestacada).HasDatabaseName("IX_Noticias_EsDestacada");
        }
    }
}
