using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.Identity;

namespace Persistence.Configurations.Identidad
{
    /// <summary>
    /// Ajusta lo que Identity ya configuró: mueve la tabla al esquema del módulo
    /// y define las columnas propias del MIDE. La clave, el correo, el hash y el
    /// control de intentos fallidos los configura Identity.
    /// </summary>
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios", Esquemas.Identidad);

            // La llave es un Guid, así que la genera la base de datos con
            // NEWSEQUENTIALID(): un Guid aleatorio como clave agrupada fragmenta
            // el índice en cada inserción.
            builder.Property(u => u.Id)
                   .HasDefaultValueSql("NEWSEQUENTIALID()")
                   .ValueGeneratedOnAdd();

            builder.Property(u => u.Nombres).HasMaxLength(100).IsRequired();
            builder.Property(u => u.Apellidos).HasMaxLength(100).IsRequired();
            builder.Property(u => u.TipoUsuario).HasConversion<int>().IsRequired();
            builder.Property(u => u.Cedula).HasMaxLength(13);
            builder.Property(u => u.FechaRegistro).IsRequired();

            builder.HasIndex(u => u.Cedula).HasDatabaseName("IX_Usuarios_Cedula");
            builder.HasIndex(u => u.TipoUsuario).HasDatabaseName("IX_Usuarios_TipoUsuario");

            builder.Ignore(u => u.NombreCompleto);
        }
    }
}
