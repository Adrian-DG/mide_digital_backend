using Domain.Common;
using Domain.Entities.Identidad;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations.Identidad
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios", Esquemas.Identidad);

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).ValueGeneratedOnAdd();

            builder.Property(u => u.Nombres).HasMaxLength(100).IsRequired();
            builder.Property(u => u.Apellidos).HasMaxLength(100).IsRequired();
            builder.Property(u => u.Correo).HasMaxLength(256).IsRequired();
            builder.Property(u => u.ContrasenaHash).HasMaxLength(512).IsRequired();
            builder.Property(u => u.TipoUsuario).HasConversion<int>().IsRequired();
            builder.Property(u => u.Cedula).HasMaxLength(13);
            builder.Property(u => u.Telefono).HasMaxLength(20);
            builder.Property(u => u.FechaRegistro).IsRequired();

            builder.HasIndex(u => u.Correo).IsUnique().HasDatabaseName("UX_Usuarios_Correo");
            builder.HasIndex(u => u.Cedula).HasDatabaseName("IX_Usuarios_Cedula");
            builder.HasIndex(u => u.TipoUsuario).HasDatabaseName("IX_Usuarios_TipoUsuario");

            builder.Ignore(u => u.NombreCompleto);
        }
    }
}
