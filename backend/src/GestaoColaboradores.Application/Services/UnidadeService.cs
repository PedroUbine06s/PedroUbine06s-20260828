using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Entidades;

namespace GestaoColaboradores.Application.Services;

public interface IUnidadeService
{
    Task<Result<UnidadeComColaboradoresDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<UnidadeRespostaDto>> CriarAsync(CriarUnidadeDto dto, CancellationToken ct = default);
    Task<Result<UnidadeRespostaDto>> AtualizarAsync(Guid id, AtualizarUnidadeDto dto, CancellationToken ct = default);
    Task<Result<UnidadeRespostaDto>> AtualizarParcialAsync(Guid id, AtualizarParcialUnidadeDto dto, CancellationToken ct = default);
    Task<Result<PaginaDto<UnidadeComColaboradoresDto>>> ListarAsync(PaginacaoQuery paginacao, CancellationToken ct = default);
}

public class UnidadeService(
    IUnidadeRepository unidadeRepo,
    IGeradorCodigo gerador,
    IUnitOfWork uow) : IUnidadeService
{
    public async Task<Result<UnidadeComColaboradoresDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var unidade = await unidadeRepo.ObterComColaboradoresAsync(id, ct);

        return unidade is null
            ? Result<UnidadeComColaboradoresDto>.Falha($"Unidade {id} não encontrada.", TipoErro.NaoEncontrado)
            : Result<UnidadeComColaboradoresDto>.Sucesso(ParaDtoComColaboradores(unidade));
    }

    public async Task<Result<UnidadeRespostaDto>> CriarAsync(CriarUnidadeDto dto, CancellationToken ct = default)
    {
        var codigo = await gerador.GerarAsync(TipoCodigo.Unidade, ct);
        var unidade = Unidade.Criar(codigo, dto.Nome);

        await unidadeRepo.AdicionarAsync(unidade, ct);
        await uow.CommitAsync(ct);

        return Result<UnidadeRespostaDto>.Sucesso(ParaDto(unidade));
    }

    public async Task<Result<UnidadeRespostaDto>> AtualizarAsync(Guid id, AtualizarUnidadeDto dto, CancellationToken ct = default)
    {
        var unidade = await unidadeRepo.ObterPorIdAsync(id, ct);

        if (unidade is null)
            return Result<UnidadeRespostaDto>.Falha($"Unidade {id} não encontrada.", TipoErro.NaoEncontrado);

        unidade.AlterarNome(dto.Nome);

        if (dto.Ativo)
            unidade.Ativar();
        else
            unidade.Inativar();

        await uow.CommitAsync(ct);

        return Result<UnidadeRespostaDto>.Sucesso(ParaDto(unidade));
    }

    /// <summary>
    /// PATCH: aplica apenas os campos informados. É por aqui que se inativa uma unidade
    /// sem precisar reenviar o nome — o caso de uso mais comum do enunciado.
    /// </summary>
    public async Task<Result<UnidadeRespostaDto>> AtualizarParcialAsync(Guid id, AtualizarParcialUnidadeDto dto, CancellationToken ct = default)
    {
        var unidade = await unidadeRepo.ObterPorIdAsync(id, ct);

        if (unidade is null)
            return Result<UnidadeRespostaDto>.Falha($"Unidade {id} não encontrada.", TipoErro.NaoEncontrado);

        if (dto.Nome is not null)
            unidade.AlterarNome(dto.Nome);

        if (dto.Ativo is not null)
        {
            if (dto.Ativo.Value)
                unidade.Ativar();
            else
                unidade.Inativar();
        }

        await uow.CommitAsync(ct);

        return Result<UnidadeRespostaDto>.Sucesso(ParaDto(unidade));
    }

    public async Task<Result<PaginaDto<UnidadeComColaboradoresDto>>> ListarAsync(
        PaginacaoQuery paginacao, CancellationToken ct = default)
    {
        var (itens, total) = await unidadeRepo.ListarComColaboradoresPaginadoAsync(paginacao, ct);

        return Result<PaginaDto<UnidadeComColaboradoresDto>>.Sucesso(
            new PaginaDto<UnidadeComColaboradoresDto>(
                itens.Select(ParaDtoComColaboradores).ToList(), paginacao.Pagina, paginacao.Tamanho, total));
    }

    private static UnidadeRespostaDto ParaDto(Unidade u) =>
        new(u.Id, u.Codigo, u.Nome, u.Ativo);


    private static UnidadeComColaboradoresDto ParaDtoComColaboradores(Unidade u) =>
        new(u.Id, u.Codigo, u.Nome, u.Ativo,
            u.Colaboradores
                .Select(c => new ColaboradorRespostaDto(
                    c.Id, c.Codigo, c.Nome,
                    u.Id, u.Codigo, u.Nome,
                    c.Usuario.Id, c.Usuario.Codigo, c.Usuario.Login))
                .ToList());
}
