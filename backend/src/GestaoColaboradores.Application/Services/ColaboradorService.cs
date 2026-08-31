using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Entidades;

namespace GestaoColaboradores.Application.Services;

public interface IColaboradorService
{
    Task<Result<ColaboradorRespostaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<Result<ColaboradorRespostaDto>> CriarAsync(CriarColaboradorDto dto, CancellationToken ct = default);
    Task<Result<ColaboradorRespostaDto>> AtualizarAsync(Guid id, AtualizarColaboradorDto dto, CancellationToken ct = default);
    Task<Result<ColaboradorRespostaDto>> AtualizarParcialAsync(Guid id, AtualizarParcialColaboradorDto dto, CancellationToken ct = default);
    Task<Result> RemoverAsync(Guid id, CancellationToken ct = default);
    Task<Result<List<ColaboradorRespostaDto>>> ListarAsync(CancellationToken ct = default);
}

/// <summary>
/// FATIA VERTICAL DE REFERÊNCIA — este service está completo de propósito:
/// mostra Result Pattern + Factory Method + Unit of Work funcionando juntos.
/// Use como modelo para UsuarioService e UnidadeService.
/// </summary>
public class ColaboradorService(
    IColaboradorRepository colaboradorRepo,
    IUnidadeRepository unidadeRepo,
    IUsuarioRepository usuarioRepo,
    IGeradorCodigo gerador,
    IUnitOfWork uow) : IColaboradorService
{
    public async Task<Result<ColaboradorRespostaDto>> ObterPorIdAsync(Guid id, CancellationToken ct = default)
    {
        var colaborador = await colaboradorRepo.ObterComUnidadeAsync(id, ct);

        return colaborador is null
            ? Result<ColaboradorRespostaDto>.Falha($"Colaborador {id} não encontrado.", TipoErro.NaoEncontrado)
            : Result<ColaboradorRespostaDto>.Sucesso(ParaDto(colaborador));
    }

    public async Task<Result<ColaboradorRespostaDto>> CriarAsync(CriarColaboradorDto dto, CancellationToken ct = default)
    {
        var unidade = await unidadeRepo.ObterPorIdAsync(dto.UnidadeId, ct);
        if (unidade is null)
            return Result<ColaboradorRespostaDto>.Falha("Unidade não encontrada.", TipoErro.NaoEncontrado);

        if (!unidade.PodeReceberColaborador)
            return Result<ColaboradorRespostaDto>.Falha(
                "Unidade inativa não permite inclusão de novos colaboradores.", TipoErro.RegraNegocio);

        var usuario = await usuarioRepo.ObterPorIdAsync(dto.UsuarioId, ct);
        if (usuario is null)
            return Result<ColaboradorRespostaDto>.Falha("Usuário não encontrado.", TipoErro.NaoEncontrado);

        var codigo = await gerador.GerarAsync(TipoCodigo.Colaborador, ct);
        var colaborador = Colaborador.Criar(codigo, dto.Nome, unidade, usuario);

        await colaboradorRepo.AdicionarAsync(colaborador, ct);
        await uow.CommitAsync(ct);

        return Result<ColaboradorRespostaDto>.Sucesso(ParaDto(colaborador));
    }

    public async Task<Result<List<ColaboradorRespostaDto>>> ListarAsync(CancellationToken ct = default)
    {
        var colaboradores = await colaboradorRepo.ListarComUnidadeAsync(ct);
        return Result<List<ColaboradorRespostaDto>>.Sucesso(colaboradores.Select(ParaDto).ToList());
    }

    public async Task<Result<ColaboradorRespostaDto>> AtualizarAsync(Guid id, AtualizarColaboradorDto dto, CancellationToken ct = default)
    {
        var colaborador = await colaboradorRepo.ObterPorIdAsync(id, ct);
        if (colaborador is null)
            return Result<ColaboradorRespostaDto>.Falha($"Colaborador {id} não encontrado.", TipoErro.NaoEncontrado);

        var unidade = await unidadeRepo.ObterPorIdAsync(dto.UnidadeId, ct);
        if (unidade is null)
            return Result<ColaboradorRespostaDto>.Falha("Unidade não encontrada.", TipoErro.NaoEncontrado);

        if (!unidade.PodeReceberColaborador)
            return Result<ColaboradorRespostaDto>.Falha(
                "Unidade inativa não pode receber colaboradores.", TipoErro.RegraNegocio);

        // As duas alterações ocorrem antes do commit: se a segunda falhasse depois de um
        // commit da primeira, o colaborador ficaria gravado pela metade.
        colaborador.AlterarNome(dto.Nome);
        colaborador.AlterarUnidade(unidade);

        await uow.CommitAsync(ct);

        return Result<ColaboradorRespostaDto>.Sucesso(ParaDto(colaborador));
    }

    /// <summary>PATCH: renomear sem reenviar a unidade, ou transferir sem reenviar o nome.</summary>
    public async Task<Result<ColaboradorRespostaDto>> AtualizarParcialAsync(Guid id, AtualizarParcialColaboradorDto dto, CancellationToken ct = default)
    {
        var colaborador = await colaboradorRepo.ObterComUnidadeAsync(id, ct);
        if (colaborador is null)
            return Result<ColaboradorRespostaDto>.Falha($"Colaborador {id} não encontrado.", TipoErro.NaoEncontrado);

        Unidade? novaUnidade = null;

        if (dto.UnidadeId is not null)
        {
            novaUnidade = await unidadeRepo.ObterPorIdAsync(dto.UnidadeId.Value, ct);

            if (novaUnidade is null)
                return Result<ColaboradorRespostaDto>.Falha("Unidade não encontrada.", TipoErro.NaoEncontrado);

            if (!novaUnidade.PodeReceberColaborador)
                return Result<ColaboradorRespostaDto>.Falha(
                    "Unidade inativa não pode receber colaboradores.", TipoErro.RegraNegocio);
        }

        // Alterações só depois de todas as validações, e todas antes do commit.
        if (dto.Nome is not null)
            colaborador.AlterarNome(dto.Nome);

        if (novaUnidade is not null)
            colaborador.AlterarUnidade(novaUnidade);

        await uow.CommitAsync(ct);

        return Result<ColaboradorRespostaDto>.Sucesso(ParaDto(colaborador));
    }

    public async Task<Result> RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var colaborador = await colaboradorRepo.ObterPorIdAsync(id, ct);
        if (colaborador is null)
            return Result.Falha($"Colaborador {id} não encontrado.", TipoErro.NaoEncontrado);

        // Decisão de domínio: o usuário vinculado é INATIVADO, não excluído. Remover o
        // colaborador encerra o acesso, mas apagar o usuário destruiria o histórico de quem
        // fez o quê — e deixá-lo ativo manteria uma credencial válida sem dono.
        var usuario = await usuarioRepo.ObterPorIdAsync(colaborador.UsuarioId, ct);
        usuario?.Inativar();

        colaboradorRepo.Remover(colaborador);
        await uow.CommitAsync(ct);

        return Result.Sucesso();
    }

    private static ColaboradorRespostaDto ParaDto(Colaborador c) =>
        new(c.Id, c.Codigo, c.Nome, c.Unidade.Id, c.Unidade.Codigo, c.Unidade.Nome);
}
