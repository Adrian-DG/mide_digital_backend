using System;
using Application.Abstraction;
using Application.Services.Catalogo;
using Application.Services.Contenido;
using Application.Services.Identidad;
using Application.Services.Retroalimentacion;
using Application.Services.Tramites;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Connections;
using Persistence.Context;
using Persistence.Security;
using Persistence.Services.Catalogo;
using Persistence.Services.Contenido;
using Persistence.Services.Identidad;
using Persistence.Services.Retroalimentacion;
using Persistence.Services.Tramites;

namespace Persistence
{
    /// <summary>
    /// Registro único de la capa de datos: contexto de escritura (EF), fábrica de
    /// conexiones de lectura (Dapper) y los servicios que implementan los
    /// contratos de Application.
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
            services.AddDbContext<MideDbContext>(options =>
                options.UseSqlServer(connectionString, sql =>
                {
                    sql.MigrationsHistoryTable("__EFMigrationsHistory", Domain.Common.Esquemas.Catalogo);
                    sql.EnableRetryOnFailure();
                }));

            // Singleton: solo guarda la cadena de conexión; cada llamada abre y
            // cierra su propia conexión.
            services.AddSingleton<ISqlConnectionFactory>(new SqlConnectionFactory(connectionString));

            services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

            return services.AddServicios();
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
