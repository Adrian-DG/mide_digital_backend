using System;
using System.Linq.Expressions;
using Domain.Abstraction;
using Domain.Entities.Catalogo;
using Domain.Entities.Contenido;
using Domain.Entities.Retroalimentacion;
using Domain.Entities.Tramites;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.Identity;

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
    /// <para>
    /// La auditoría no se resuelve aquí sobreescribiendo SaveChanges: la aplica
    /// un interceptor registrado al configurar el contexto, así el contexto no
    /// necesita conocer al usuario actual ni al reloj.
    /// </para>
    /// </remarks>
    public class MideDbContext : IdentityUserContext<Usuario, Guid>
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
    }
}
