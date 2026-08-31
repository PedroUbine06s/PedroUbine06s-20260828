using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Entidades;

namespace GestaoColaboradores.Application.Services;

public interface IUsuarioService
{
    Task<Result<UsuarioRespostaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<UsuarioRespostaDto>> CriarAsync(CriarUsuarioDto dto, CancellationToken ct = default);
    Task<Result<UsuarioRespostaDto>> AtualizarAsync(Guid id, AtualizarUsuarioDto dto, CancellationToken ct = default);
    Task<Result<UsuarioRespostaDto>> AtualizarParcialAsync(Guid id, AtualizarParcialUsuarioDto dto, CancellationToken ct = default);
    /// <param name="ativo">null = todos; true/false = filtro por status (requisito do enunciado).</param>
    Task<Result<PaginaDto<UsuarioRespostaDto>>> ListarAsync(
        bool? ativo, PaginacaoQuery paginacao, CancellationToken ct = default);
}

public class UsuarioService(
    IUsuarioRepository usuarioRepo,
    IPasswordHasher hasher,
    IGeradorCodigo gerador,
    IUnitOfWork uow) : IUsuarioService
{
    public async Task<Result<UsuarioRespostaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var usuario = await usuarioRepo.ObterPorIdAsync(id, ct);

        return usuario is null
            ? Result<UsuarioRespostaDto>.Falha($"Usuário {id} não encontrado.", TipoErro.NaoEncontrado)
            : Result<UsuarioRespostaDto>.Sucesso(ParaDto(usuario));
    }

    public async Task<Result<UsuarioRespostaDto>> CriarAsync(CriarUsuarioDto dto, CancellationToken ct = default)
    {

        if (await usuarioRepo.ExisteLoginAsync(dto.Login, ct))
            return Result<UsuarioRespostaDto>.Falha(
                $"Já existe um usuário com o login '{dto.Login}'.", TipoErro.Conflito);

        var codigo = await gerador.GerarAsync(TipoCodigo.Usuario, ct);
        var usuario = Usuario.Criar(codigo, dto.Login, hasher.Hash(dto.Senha));

        if (!dto.Ativo) usuario.Inativar();

        await usuarioRepo.AdicionarAsync(usuario, ct);
        await uow.CommitAsync(ct);

        return Result<UsuarioRespostaDto>.Sucesso(ParaDto(usuario));
    }

    public async Task<Result<UsuarioRespostaDto>> AtualizarAsync(Guid id, AtualizarUsuarioDto dto, CancellationToken ct = default)
    {
        var usuario = await usuarioRepo.ObterPorIdAsync(id, ct);

        if (usuario is null)
            return Result<UsuarioRespostaDto>.Falha($"Usuário {id} não encontrado.", TipoErro.NaoEncontrado);

        if (!string.IsNullOrWhiteSpace(dto.Senha))
            usuario.AlterarSenha(hasher.Hash(dto.Senha));

        if (dto.Ativo)
            usuario.Ativar();
        else
            usuario.Inativar();

        await uow.CommitAsync(ct);

        return Result<UsuarioRespostaDto>.Sucesso(ParaDto(usuario));
    }

    /// <summary>PATCH: aplica apenas os campos informados; os nulos ficam como estão.</summary>
    public async Task<Result<UsuarioRespostaDto>> AtualizarParcialAsync(Guid id, AtualizarParcialUsuarioDto dto, CancellationToken ct = default)
    {
        var usuario = await usuarioRepo.ObterPorIdAsync(id, ct);

        if (usuario is null)
            return Result<UsuarioRespostaDto>.Falha($"Usuário {id} não encontrado.", TipoErro.NaoEncontrado);

        if (!string.IsNullOrWhiteSpace(dto.Senha))
            usuario.AlterarSenha(hasher.Hash(dto.Senha));

        if (dto.Ativo is not null)
        {
            if (dto.Ativo.Value)
                usuario.Ativar();
            else
                usuario.Inativar();
        }

        await uow.CommitAsync(ct);

        return Result<UsuarioRespostaDto>.Sucesso(ParaDto(usuario));
    }

    public async Task<Result<PaginaDto<UsuarioRespostaDto>>> ListarAsync(
        bool? ativo, PaginacaoQuery paginacao, CancellationToken ct = default)
    {
        var (itens, total) = await usuarioRepo.ListarPaginadoAsync(ativo, paginacao, ct);

        return Result<PaginaDto<UsuarioRespostaDto>>.Sucesso(
            new PaginaDto<UsuarioRespostaDto>(
                itens.Select(ParaDto).ToList(), paginacao.Pagina, paginacao.Tamanho, total));
    }

    private static UsuarioRespostaDto ParaDto(Usuario u) =>
        new(u.Id, u.Codigo, u.Login, u.Ativo);
}
