using System;
using Application.Abstraction;
using Application.Services.Catalogo;
using Application.Services.Contenido;
using Application.Services.Identidad;
using Application.Services.Retroalimentacion;
using Application.Services.Tramites;
using Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Persistence.Connections;
using Persistence.Context;
using Persistence.Identity;
using Persistence.Interceptors;
using Persistence.Seeding;
using Persistence.Services.Catalogo;
using Persistence.Services.Contenido;
using Persistence.Services.Identidad;
using Persistence.Services.Retroalimentacion;
using Persistence.Services.Tramites;

namespace Persistence
{
    /// <summary>
    /// Registro único de la capa de datos: contexto de escritura (EF) con sus
    /// interceptores, Identity, fábrica de conexiones de lectura (Dapper) y los
    /// servicios que implementan los contratos de Application.
    /// </summary>
    public static class DependencyInjection
    {
        public const string NombreCadenaConexion = "MideDigital";

        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(NombreCadenaConexion)
                ?? throw new InvalidOperationException(
                    $"No se encontró la cadena de conexión '{NombreCadenaConexion}' en la configuración.");

            return services.AddPersistence(connectionString);
        }

        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
        {
            // TryAdd: si Presentation ya registró su propio reloj o su propio
            // proveedor de usuario actual (el que lee el token), esos ganan.
            services.TryAddSingleton(TimeProvider.System);
            services.TryAddScoped<IUsuarioActual, UsuarioActualNoIdentificado>();

            services.AddScoped<AuditableEntityInterceptor>();

            services.AddDbContext<MideDbContext>((proveedor, options) =>
                options
                    .UseSqlServer(connectionString, sql =>
                    {
                        sql.MigrationsAssembly(typeof(MideDbContext).Assembly.FullName);
                        sql.MigrationsHistoryTable("__EFMigrationsHistory", Esquemas.Catalogo);
                        sql.EnableRetryOnFailure();
                    })
                    .AddInterceptors(proveedor.GetRequiredService<AuditableEntityInterceptor>()));

            // Singleton: solo guarda la cadena de conexión; cada llamada abre y
            // cierra su propia conexión.
            services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));

            // Se registra siempre, pero solo el host decide ejecutarlo (en desarrollo).
            services.AddScoped<SembradorUsuariosDePrueba>();

            return services
                .AddIdentidad()
                .AddServicios();
        }

        /// <summary>
        /// AddIdentityCore y no AddIdentity: sin cookies ni SignInManager, porque
        /// la API autentica por token y la emisión vive en Presentation.
        /// Los proveedores de token (confirmación de correo, restablecimiento)
        /// requieren Data Protection, que el host web registra por su cuenta.
        /// </summary>
        private static IServiceCollection AddIdentidad(this IServiceCollection services)
        {
            services
                .AddIdentityCore<Usuario>(opciones =>
                {
                    opciones.User.RequireUniqueEmail = true;

                    opciones.Password.RequiredLength = 10;
                    opciones.Password.RequireDigit = true;
                    opciones.Password.RequireUppercase = true;
                    opciones.Password.RequireLowercase = true;
                    opciones.Password.RequireNonAlphanumeric = true;

                    opciones.Lockout.MaxFailedAccessAttempts = 5;
                    opciones.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                    // El ciudadano se registra desde la app; exigir el correo
                    // confirmado para poder tramitar se decide en Presentation.
                    opciones.SignIn.RequireConfirmedEmail = false;
                })
                .AddEntityFrameworkStores<MideDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }

        /// <summary>
        /// Las implementaciones de los contratos de Application viven en esta capa,
        /// así que se registran aquí y no en Application.
        /// </summary>
        private static IServiceCollection AddServicios(this IServiceCollection services)
        {
            services.AddScoped<IUsuarioService, UsuarioService>();

            services.AddScoped<IDependenciaService, DependenciaService>();
            services.AddScoped<ICategoriaServicioService, CategoriaServicioService>();
            services.AddScoped<IServicioService, ServicioService>();

            services.AddScoped<ISolicitudService, SolicitudService>();

            services.AddScoped<INoticiaService, NoticiaService>();

            services.AddScoped<ICalificacionServicioService, CalificacionServicioService>();

            return services;
        }
    }
}
