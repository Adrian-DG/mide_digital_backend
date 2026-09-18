using Application.DTOs.Identidad;

namespace API.Contracts.Autenticacion;

/// <param name="TipoToken">Esquema con el que el cliente debe enviarlo: Bearer.</param>
public record SesionResponse(
    string Token,
    string TipoToken,
    DateTime ExpiraEn,
    UsuarioDto Usuario);

/// <summary>
/// Respuesta de los endpoints que generan un token de correo. El token solo se
/// devuelve en desarrollo: en producción lo envía el servicio de correo y el
/// cliente nunca lo ve.
/// </summary>
public record TokenCorreoResponse(string Mensaje, string? Token);
