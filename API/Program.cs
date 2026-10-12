using System.Text;
using API.Authentication;
using API.Middleware;
using Application.Abstraction;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Persistence.Seeding;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManejadorDeExcepciones>();

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SeccionConfiguracion))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IGeneradorTokens, GeneradorTokensJwt>();

// Antes de AddPersistence: esa capa registra su implementación de reserva con
// TryAdd, así que la que lee el token tiene que estar primero para que el
// interceptor de auditoría sepa quién hace la operación.
builder.Services.AddScoped<IUsuarioActual, UsuarioActual>();

builder.Services.AddPersistence(builder.Configuration);

var jwt = builder.Configuration
    .GetSection(JwtOptions.SeccionConfiguracion)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        $"Falta la sección de configuración '{JwtOptions.SeccionConfiguracion}'.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        // Los claims llegan con su nombre original ("sub" no se traduce a
        // ClaimTypes.NameIdentifier), que es lo que leen UsuarioActual y el
        // controlador.
        opciones.MapInboundClaims = false;
        opciones.RequireHttpsMetadata = !builder.Environment.IsDevelopment();

        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.ClaveDeFirma)),
            ValidateLifetime = true,
            // El valor por omisión son 5 minutos de tolerancia, demasiado para
            // tokens de una hora.
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Usuarios para probar el login (ver Persistence/Seeding/UsuariosDePrueba.cs).
    // Doble condición a propósito: nunca en otro entorno, y se puede apagar en local.
    if (app.Configuration.GetValue<bool>("DatosDePrueba:Sembrar"))
    {
        await using var alcance = app.Services.CreateAsyncScope();
        await alcance.ServiceProvider.GetRequiredService<SembradorUsuariosDePrueba>().SembrarAsync();
    }
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
