using Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations.Identidad
{
    /// <summary>
    /// Tablas auxiliares de Identity. Se renombran y se mueven al esquema
    /// <c>identidad</c> para que no aparezcan como AspNetUserClaims en dbo.
    /// </summary>
    public class UsuarioClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<int>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserClaim<int>> builder)
        {
            builder.ToTable("UsuariosClaims", Esquemas.Identidad);
        }
    }

    public class UsuarioLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<int>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserLogin<int>> builder)
        {
            builder.ToTable("UsuariosLogins", Esquemas.Identidad);
        }
    }

    public class UsuarioTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<int>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserToken<int>> builder)
        {
            builder.ToTable("UsuariosTokens", Esquemas.Identidad);
        }
    }
}
