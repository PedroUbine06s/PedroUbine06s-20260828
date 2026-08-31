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
    public Task<Result<TokenRespostaDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        // TODO:
        // 1. ObterPorLoginAsync — se nulo, falha NaoAutorizado com mensagem GENÉRICA
        //    ("login ou senha inválidos" — nunca revelar qual dos dois errou)
        // 2. hasher.Verificar(dto.Senha, usuario.SenhaHash) — mesma mensagem genérica
        // 3. REGRA: somente usuário ATIVO loga (mensagem específica aqui é aceitável)
        // 4. tokenService.GerarToken(usuario) → TokenRespostaDto
        throw new NotImplementedException();
    }
}
