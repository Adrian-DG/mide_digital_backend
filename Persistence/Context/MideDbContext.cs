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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Context
{
    /// <summary>
    /// Modelo de escritura. Las lecturas van por Dapper, asi que aqui no se
    /// definen proyecciones ni vistas.
    /// </summary>
    public class MideDbContext : DbContext
    {
        public MideDbContext(DbContextOptions<MideDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();

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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MideDbContext).Assembly);

            AplicarFiltroDeBorradoLogico(modelBuilder);

            base.OnModelCreating(modelBuilder);
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
        /// el filtro se aplica por convencion a toda entidad que herede de
        /// <see cref="BaseEntityMetadata"/>.
        /// </summary>
        private static void AplicarFiltroDeBorradoLogico(ModelBuilder modelBuilder)
        {
            foreach (var tipo in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(BaseEntityMetadata).IsAssignableFrom(tipo.ClrType))
                {
                    continue;
                }

                var parametro = Expression.Parameter(tipo.ClrType, "e");
                var propiedad = Expression.Property(parametro, nameof(BaseEntityMetadata.IsDeleted));
                var filtro = Expression.Lambda(Expression.Not(propiedad), parametro);

                modelBuilder.Entity(tipo.ClrType).HasQueryFilter(filtro);
            }
        }

        /// <summary>
        /// Marca las fechas de auditoria. El usuario responsable lo fija el
        /// servicio, que es quien sabe en nombre de quien actua.
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
