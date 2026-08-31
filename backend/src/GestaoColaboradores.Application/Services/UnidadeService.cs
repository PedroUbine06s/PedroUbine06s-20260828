using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Entidades;

namespace GestaoColaboradores.Application.Services;

public interface IUnidadeService
{
    Task<Result<UnidadeRespostaDto>> CriarAsync(CriarUnidadeDto dto, CancellationToken ct = default);
    Task<Result<UnidadeRespostaDto>> AtualizarAsync(int id, AtualizarUnidadeDto dto, CancellationToken ct = default);
    Task<Result<UnidadeRespostaDto>> AtualizarParcialAsync(int id, AtualizarParcialUnidadeDto dto, CancellationToken ct = default);
    /// <summary>Requisito: listar unidades COM seus colaboradores.</summary>
    Task<Result<List<UnidadeComColaboradoresDto>>> ListarAsync(CancellationToken ct = default);
}

public class UnidadeService(IUnidadeRepository unidadeRepo, IUnitOfWork uow) : IUnidadeService
{
    public async Task<Result<UnidadeRespostaDto>> CriarAsync(CriarUnidadeDto dto, CancellationToken ct = default)
    {
        if (await unidadeRepo.ExisteCodigoAsync(dto.Codigo, ct))
            return Result<UnidadeRespostaDto>.Falha($"Já existe uma unidade com o código '{dto.Codigo}'.", TipoErro.Conflito);

        var unidade = Unidade.Criar(dto.Codigo, dto.Nome);

        await unidadeRepo.AdicionarAsync(unidade, ct);
        await uow.CommitAsync(ct);

        return Result<UnidadeRespostaDto>.Sucesso(ParaDto(unidade));
    }

    public async Task<Result<UnidadeRespostaDto>> AtualizarAsync(int id, AtualizarUnidadeDto dto, CancellationToken ct = default)
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
    public async Task<Result<UnidadeRespostaDto>> AtualizarParcialAsync(int id, AtualizarParcialUnidadeDto dto, CancellationToken ct = default)
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

    public async Task<Result<List<UnidadeComColaboradoresDto>>> ListarAsync(CancellationToken ct = default)
    {
        var unidades = await unidadeRepo.ListarComColaboradoresAsync(ct);

        var dtos = unidades
            .Select(u => new UnidadeComColaboradoresDto(
                u.Id,
                u.Codigo,
                u.Nome,
                u.Ativo,
                u.Colaboradores
                    .Select(c => new ColaboradorRespostaDto(c.Id, c.Codigo, c.Nome, u.Codigo, u.Nome))
                    .ToList()))
            .ToList();

        return Result<List<UnidadeComColaboradoresDto>>.Sucesso(dtos);
    }

    private static UnidadeRespostaDto ParaDto(Unidade u) =>
        new(u.Id, u.Codigo, u.Nome, u.Ativo);
}
