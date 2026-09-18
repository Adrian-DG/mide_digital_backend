using Application.DTOs.Identidad;

namespace API.Authentication;

public interface IGeneradorTokens
{
    TokenEmitido Emitir(UsuarioDto usuario);
}

/// <param name="Token">JWT firmado.</param>
/// <param name="ExpiraEn">Instante de expiración, en UTC.</param>
public record TokenEmitido(string Token, DateTime ExpiraEn);
