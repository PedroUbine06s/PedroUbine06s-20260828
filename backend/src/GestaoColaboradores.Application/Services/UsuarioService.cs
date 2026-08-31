using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Entidades;

namespace GestaoColaboradores.Application.Services;

public interface IUsuarioService
{
    Task<Result<UsuarioRespostaDto>> CriarAsync(CriarUsuarioDto dto, CancellationToken ct = default);
    Task<Result<UsuarioRespostaDto>> AtualizarAsync(int id, AtualizarUsuarioDto dto, CancellationToken ct = default);
    /// <param name="ativo">null = todos; true/false = filtro por status (requisito do enunciado).</param>
    Task<Result<List<UsuarioRespostaDto>>> ListarAsync(bool? ativo, CancellationToken ct = default);
}

public class UsuarioService(
    IUsuarioRepository usuarioRepo,
    IPasswordHasher hasher,          // Strategy: nunca armazene a senha em texto plano
    IUnitOfWork uow) : IUsuarioService
{
    public async Task<Result<UsuarioRespostaDto>> CriarAsync(CriarUsuarioDto dto, CancellationToken ct = default)
    {
        if(await usuarioRepo.ExisteCodigoAsync(dto.Codigo, ct))
            return Result<UsuarioRespostaDto>.Falha($"Já existe um usuário com o código '{dto.Codigo}'", TipoErro.Conflito);
        if(await usuarioRepo.ExisteLoginAsync(dto.Login, ct))
            return Result<UsuarioRespostaDto>.Falha($"Já existe um usuário com o login '{dto.Login}'", TipoErro.Conflito);

        var usuario = Usuario.Criar(dto.Codigo, dto.Login, hasher.Hash(dto.Senha));

        if(!dto.Ativo) usuario.Inativar();

        await usuarioRepo.AdicionarAsync(usuario,ct);
        await uow.CommitAsync(ct);

        return Result<UsuarioRespostaDto>.Sucesso(ParaDto(usuario));
    }

    public Task<Result<UsuarioRespostaDto>> AtualizarAsync(int id, AtualizarUsuarioDto dto, CancellationToken ct = default)
    {
        // TODO: buscar por id (404), aplicar SOMENTE senha (se informada, via hasher) e status.
        throw new NotImplementedException();
    }

    public Task<Result<List<UsuarioRespostaDto>>> ListarAsync(bool? ativo, CancellationToken ct = default)
    {
        // TODO: ativo is null ? ListarAsync : ListarPorStatusAsync
        throw new NotImplementedException();
    }

    private static UsuarioRespostaDto ParaDto(Usuario u) =>
        new (u.Id, u.Codigo, u.Login, u.Ativo);
}
