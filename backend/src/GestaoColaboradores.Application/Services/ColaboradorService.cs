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
    Task<Result<PaginaDto<ColaboradorRespostaDto>>> ListarAsync(PaginacaoQuery paginacao, CancellationToken ct = default);
}


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

        // Cada usuário pertence a um único colaborador (ver "Decisões de domínio" no README).
        if (await colaboradorRepo.ExisteParaUsuarioAsync(dto.UsuarioId, ct))
            return Result<ColaboradorRespostaDto>.Falha(
                "Este usuário já está vinculado a outro colaborador.", TipoErro.Conflito);

        var codigo = await gerador.GerarAsync(TipoCodigo.Colaborador, ct);
        var colaborador = Colaborador.Criar(codigo, dto.Nome, unidade, usuario);

        await colaboradorRepo.AdicionarAsync(colaborador, ct);
        await uow.CommitAsync(ct);

        return Result<ColaboradorRespostaDto>.Sucesso(ParaDto(colaborador));
    }

    public async Task<Result<PaginaDto<ColaboradorRespostaDto>>> ListarAsync(
        PaginacaoQuery paginacao, CancellationToken ct = default)
    {
        var (itens, total) = await colaboradorRepo.ListarComUnidadePaginadoAsync(paginacao, ct);

        return Result<PaginaDto<ColaboradorRespostaDto>>.Sucesso(
            new PaginaDto<ColaboradorRespostaDto>(
                itens.Select(ParaDto).ToList(), paginacao.Pagina, paginacao.Tamanho, total));
    }

    public async Task<Result<ColaboradorRespostaDto>> AtualizarAsync(Guid id, AtualizarColaboradorDto dto, CancellationToken ct = default)
    {
        // Com Include: quando não há transferência, a navegação não é preenchida por
        // AlterarUnidade e montar o DTO de resposta acessaria referência nula.
        var colaborador = await colaboradorRepo.ObterComUnidadeAsync(id, ct);
        if (colaborador is null)
            return Result<ColaboradorRespostaDto>.Falha($"Colaborador {id} não encontrado.", TipoErro.NaoEncontrado);

        var unidade = await unidadeRepo.ObterPorIdAsync(dto.UnidadeId, ct);
        if (unidade is null)
            return Result<ColaboradorRespostaDto>.Falha("Unidade não encontrada.", TipoErro.NaoEncontrado);

        // A regra é que unidade inativa não RECEBE colaborador; quem já está lá não está
        // sendo recebido. Validar sem comparar impediria até renomear alguém de uma unidade
        // inativa, contradizendo o fato de que inativar não desvincula quem já estava.
        if (colaborador.UnidadeId != dto.UnidadeId && !unidade.PodeReceberColaborador)
            return Result<ColaboradorRespostaDto>.Falha(
                "Unidade inativa não pode receber colaboradores.", TipoErro.RegraNegocio);


        colaborador.AlterarNome(dto.Nome);

        // AlterarUnidade é transferência e recusa unidade inativa no próprio domínio.
        // Chamá-la com a unidade que já está lá lançaria por uma transferência inexistente.
        if (colaborador.UnidadeId != dto.UnidadeId)
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

            // Mesma comparação do PUT: reafirmar a unidade atual não é transferência, e as
            // duas rotas precisam concordar diante do mesmo corpo.
            if (colaborador.UnidadeId != dto.UnidadeId.Value && !novaUnidade.PodeReceberColaborador)
                return Result<ColaboradorRespostaDto>.Falha(
                    "Unidade inativa não pode receber colaboradores.", TipoErro.RegraNegocio);
        }

        if (dto.Nome is not null)
            colaborador.AlterarNome(dto.Nome);

        if (novaUnidade is not null && colaborador.UnidadeId != novaUnidade.Id)
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
        new(c.Id, c.Codigo, c.Nome,
            c.Unidade.Id, c.Unidade.Codigo, c.Unidade.Nome,
            c.Usuario.Id, c.Usuario.Codigo, c.Usuario.Login);
}
