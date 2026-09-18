using Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API.Middleware;

/// <summary>
/// Traduce las excepciones de la capa de aplicación a respuestas HTTP con
/// ProblemDetails, para que los servicios no tengan que conocer códigos de
/// estado y los controladores no repitan try/catch.
/// </summary>
public class ManejadorDeExcepciones : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<ManejadorDeExcepciones> _logger;

    public ManejadorDeExcepciones(IProblemDetailsService problemDetails, ILogger<ManejadorDeExcepciones> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (estado, titulo) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            CuentaBloqueadaException => (StatusCodes.Status423Locked, "Cuenta bloqueada"),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Solicitud no válida"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno")
        };

        if (estado == StatusCodes.Status500InternalServerError)
        {
            // Solo lo inesperado se registra como error; una regla de negocio
            // incumplida es una respuesta normal, no una falla del servidor.
            _logger.LogError(exception, "Error no controlado procesando {Ruta}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = estado;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = estado,
                Title = titulo,
                // El detalle de un 500 no sale al cliente: puede llevar datos
                // internos. El resto son mensajes escritos para el usuario.
                Detail = estado == StatusCodes.Status500InternalServerError
                    ? "Ocurrió un error procesando la solicitud."
                    : exception.Message
            }
        });
    }
}
