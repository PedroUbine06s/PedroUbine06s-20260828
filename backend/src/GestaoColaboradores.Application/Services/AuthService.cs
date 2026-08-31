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
    public async  Task<Result<TokenRespostaDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        const string credenciaisInvalidas = "Login ou senha inválidos.";

        var usuario = await usuarioRepo.ObterPorLoginAsync(dto.Login, ct);

        if (usuario is null)
            return Result<TokenRespostaDto>.Falha(credenciaisInvalidas, TipoErro.NaoAutorizado);

        if (!hasher.Verificar(dto.Senha, usuario.SenhaHash))
            return Result<TokenRespostaDto>.Falha(credenciaisInvalidas, TipoErro.NaoAutorizado);

        if (!usuario.Ativo)
            return Result<TokenRespostaDto>.Falha("Usuário inativo.", TipoErro.NaoAutorizado);

        var token = tokenService.GerarToken(usuario);

        return Result<TokenRespostaDto>.Sucesso(new TokenRespostaDto(token.Token, token.ExpiraEm));
    }
}
