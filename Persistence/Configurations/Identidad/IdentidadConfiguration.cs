using System;
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
    public class UsuarioClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<Guid>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder)
        {
            builder.ToTable("UsuariosClaims", Esquemas.Identidad);
        }
    }

    public class UsuarioLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<Guid>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
        {
            builder.ToTable("UsuariosLogins", Esquemas.Identidad);
        }
    }

    public class UsuarioTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
        {
            builder.ToTable("UsuariosTokens", Esquemas.Identidad);
        }
    }
}
