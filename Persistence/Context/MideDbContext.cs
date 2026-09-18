using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Domain.Abstraction;
using Domain.Entities.Catalogo;
using Domain.Entities.Contenido;
using Domain.Entities.Identidad;
using Domain.Entities.Retroalimentacion;
using Domain.Entities.Tramites;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Context
{
    /// <summary>
    /// Modelo de escritura. Las lecturas van por Dapper, así que aquí no se
    /// definen proyecciones ni vistas.
    /// </summary>
    /// <remarks>
    /// Hereda de <see cref="IdentityUserContext{TUser, TKey}"/> y no de
    /// IdentityDbContext: sin roles configurados, las cuatro tablas de roles
    /// sobrarían. Cambiar a roles más adelante es cambiar la clase base y
    /// generar una migración.
    /// </remarks>
    public class MideDbContext : IdentityUserContext<Usuario, int>
    {
        public MideDbContext(DbContextOptions<MideDbContext> options) : base(options)
        {
        }

        /// <summary>Alias del DbSet <c>Users</c> que declara Identity.</summary>
        public DbSet<Usuario> Usuarios => Users;

        public DbSet<Dependencia> Dependencias => Set<Dependencia>();
        public DbSet<CategoriaServicio> CategoriasServicio => Set<CategoriaServicio>();
        public DbSet<Servicio> Servicios => Set<Servicio>();
        public DbSet<Requisito> Requisitos => Set<Requisito>();

        public DbSet<Solicitud> Solicitudes => Set<Solicitud>();
        public DbSet<DocumentoSolicitud> DocumentosSolicitud => Set<DocumentoSolicitud>();

        public DbSet<Noticia> Noticias => Set<Noticia>();

        public DbSet<CalificacionServicio> CalificacionesServicio => Set<CalificacionServicio>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity primero: nuestras configuraciones ajustan lo que él define
            // (nombre de tabla, esquema y columnas propias del MIDE).
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MideDbContext).Assembly);

            AplicarFiltroDeBorradoLogico(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SellarAuditoria();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            SellarAuditoria();
            return base.SaveChanges();
        }

        /// <summary>
        /// Ninguna consulta debe devolver registros marcados como eliminados;
        /// el filtro se aplica por convención a toda entidad que implemente
        /// <see cref="ISoftDeletable"/>, incluido el usuario de Identity.
        /// </summary>
        private static void AplicarFiltroDeBorradoLogico(ModelBuilder modelBuilder)
        {
            foreach (var tipo in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(ISoftDeletable).IsAssignableFrom(tipo.ClrType))
                {
                    continue;
                }

                var parametro = Expression.Parameter(tipo.ClrType, "e");
                var propiedad = Expression.Property(parametro, nameof(ISoftDeletable.IsDeleted));
                var filtro = Expression.Lambda(Expression.Not(propiedad), parametro);

                modelBuilder.Entity(tipo.ClrType).HasQueryFilter(filtro);
            }
        }

        /// <summary>
        /// Marca las fechas de auditoría. El usuario responsable lo fija el
        /// servicio, que es quien sabe en nombre de quién actúa.
        /// </summary>
        private void SellarAuditoria()
        {
            var ahora = DateTime.UtcNow;

            foreach (var entrada in ChangeTracker.Entries<IAuditableEntityMetadata>())
            {
                switch (entrada.State)
                {
                    case EntityState.Added:
                        entrada.Entity.CreatedAt = ahora;
                        entrada.Entity.UpdatedAt = ahora;
                        break;

                    case EntityState.Modified:
                        entrada.Entity.UpdatedAt = ahora;
                        entrada.Property(nameof(IAuditableEntityMetadata.CreatedAt)).IsModified = false;
                        entrada.Property(nameof(IAuditableEntityMetadata.CreatedBy)).IsModified = false;
                        break;
                }
            }
        }
    }
}
