using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;

namespace GestaoColaboradores.Application.Services;

public interface IAuthService
{
    Task<Result<TokenRespostaDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
}

/// <summary>
/// O diferencial de auth amarrado ao domínio: autentica contra a tabela de Usuários do sistema.
/// </summary>
public class AuthService(
    IUsuarioRepository usuarioRepo,
    IPasswordHasher hasher,
    ITokenService tokenService) : IAuthService
{
    public async Task<Result<TokenRespostaDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        const string credenciaisInvalidas = "Login ou senha inválidos.";

        var usuario = await usuarioRepo.ObterPorLoginAsync(dto.Login, ct);

        // A verificação roda SEMPRE, exista o usuário ou não, para que os dois caminhos
        // gastem o mesmo tempo.
        var senhaConfere = hasher.Verificar(dto.Senha, usuario?.SenhaHash ?? hasher.HashDescartavel);

        if (usuario is null || !senhaConfere)
            return Result<TokenRespostaDto>.Falha(credenciaisInvalidas, TipoErro.NaoAutorizado);

        // Mensagem específica aqui é aceitável: quem chegou até este ponto já provou conhecer
        // a senha, então nada é revelado a quem não deveria saber.
        if (!usuario.Ativo)
            return Result<TokenRespostaDto>.Falha(
                "Usuário inativo. Procure um administrador.", TipoErro.NaoAutorizado);

        var token = tokenService.GerarToken(usuario);

        return Result<TokenRespostaDto>.Sucesso(new TokenRespostaDto(token.Token, token.ExpiraEm));
    }
}
