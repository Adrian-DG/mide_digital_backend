using Domain.Common;
using Domain.Entities.Tramites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations.Tramites
{
    public class DocumentoSolicitudConfiguration : IEntityTypeConfiguration<DocumentoSolicitud>
    {
        public void Configure(EntityTypeBuilder<DocumentoSolicitud> builder)
        {
            builder.ToTable("DocumentosSolicitud", Esquemas.Tramites);

            builder.HasKey(d => d.Id);
            builder.Property(d => d.Id).ValueGeneratedOnAdd();

            builder.Property(d => d.NombreArchivo).HasMaxLength(260).IsRequired();
            builder.Property(d => d.RutaArchivo).HasMaxLength(1000).IsRequired();
            builder.Property(d => d.TipoContenido).HasMaxLength(150).IsRequired();
            builder.Property(d => d.TamanoBytes).IsRequired();
            builder.Property(d => d.FechaCarga).IsRequired();

            builder.HasIndex(d => new { d.SolicitudId, d.RequisitoId })
                   .HasDatabaseName("IX_DocumentosSolicitud_Solicitud_Requisito");

            // FK a catalogo.Requisitos: cruza de esquema a proposito. Restrict para
            // que borrar un requisito no arrastre la evidencia de tramites cerrados.
            builder.HasOne(d => d.Requisito)
                   .WithMany(r => r.Documentos)
                   .HasForeignKey(d => d.RequisitoId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
