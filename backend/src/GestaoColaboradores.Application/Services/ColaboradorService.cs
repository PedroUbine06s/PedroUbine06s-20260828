using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Entidades;

namespace GestaoColaboradores.Application.Services;

public interface IColaboradorService
{
    Task<Result<ColaboradorRespostaDto>> CriarAsync(CriarColaboradorDto dto, CancellationToken ct = default);
    Task<Result<ColaboradorRespostaDto>> AtualizarAsync(int id, AtualizarColaboradorDto dto, CancellationToken ct = default);
    Task<Result> RemoverAsync(int id, CancellationToken ct = default);
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
    IUnitOfWork uow) : IColaboradorService
{
    public async Task<Result<ColaboradorRespostaDto>> CriarAsync(CriarColaboradorDto dto, CancellationToken ct = default)
    {
        if (await colaboradorRepo.ExisteCodigoAsync(dto.Codigo, ct))
            return Result<ColaboradorRespostaDto>.Falha(
                $"Já existe um colaborador com o código '{dto.Codigo}'.", TipoErro.Conflito);

        var unidade = await unidadeRepo.ObterPorCodigoAsync(dto.CodigoUnidade, ct);
        if (unidade is null)
            return Result<ColaboradorRespostaDto>.Falha("Unidade não encontrada.", TipoErro.NaoEncontrado);

        if (!unidade.PodeReceberColaborador)
            return Result<ColaboradorRespostaDto>.Falha(
                "Unidade inativa não permite inclusão de novos colaboradores.", TipoErro.RegraNegocio);

        var usuario = await usuarioRepo.ObterPorCodigoAsync(dto.CodigoUsuario, ct);
        if (usuario is null)
            return Result<ColaboradorRespostaDto>.Falha("Usuário não encontrado.", TipoErro.NaoEncontrado);

        var colaborador = Colaborador.Criar(dto.Codigo, dto.Nome, unidade, usuario);

        await colaboradorRepo.AdicionarAsync(colaborador, ct);
        await uow.CommitAsync(ct);

        return Result<ColaboradorRespostaDto>.Sucesso(ParaDto(colaborador));
    }

    public async Task<Result<List<ColaboradorRespostaDto>>> ListarAsync(CancellationToken ct = default)
    {
        var colaboradores = await colaboradorRepo.ListarComUnidadeAsync(ct);
        return Result<List<ColaboradorRespostaDto>>.Sucesso(colaboradores.Select(ParaDto).ToList());
    }

    public Task<Result<ColaboradorRespostaDto>> AtualizarAsync(int id, AtualizarColaboradorDto dto, CancellationToken ct = default)
    {
        // TODO: buscar por id (404 se não existir), buscar nova unidade (404 se não existir),
        //       chamar colaborador.AlterarNome(...) e colaborador.AlterarUnidade(...), commitar.
        //       Chame os dois ANTES do CommitAsync: nunca commite entre eles, ou uma falha
        //       na segunda alteração deixaria a primeira gravada.
        throw new NotImplementedException();
    }

    public Task<Result> RemoverAsync(int id, CancellationToken ct = default)
    {
        // TODO: buscar por id (404 se não existir), remover e commitar.
        // DECISÃO A DOCUMENTAR NO README: o que fazer com o usuário vinculado?
        // Sugestão: inativá-lo junto (usuario.Inativar()) para não deixar acesso órfão.
        throw new NotImplementedException();
    }

    private static ColaboradorRespostaDto ParaDto(Colaborador c) =>
        new(c.Id, c.Codigo, c.Nome, c.Unidade.Codigo, c.Unidade.Nome);
}
