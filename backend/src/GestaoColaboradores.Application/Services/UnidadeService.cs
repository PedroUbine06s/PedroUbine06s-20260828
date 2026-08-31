using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;

namespace GestaoColaboradores.Application.Services;

public interface IUnidadeService
{
    Task<Result<UnidadeRespostaDto>> CriarAsync(CriarUnidadeDto dto, CancellationToken ct = default);
    Task<Result<UnidadeRespostaDto>> AtualizarAsync(int id, AtualizarUnidadeDto dto, CancellationToken ct = default);
    /// <summary>Requisito: listar unidades COM seus colaboradores.</summary>
    Task<Result<List<UnidadeComColaboradoresDto>>> ListarAsync(CancellationToken ct = default);
}

public class UnidadeService(IUnidadeRepository unidadeRepo, IUnitOfWork uow) : IUnidadeService
{
    public Task<Result<UnidadeRespostaDto>> CriarAsync(CriarUnidadeDto dto, CancellationToken ct = default)
    {
        // TODO: ExisteCodigoAsync → 409; Unidade.Criar; Adicionar + Commit
        throw new NotImplementedException();
    }

    public Task<Result<UnidadeRespostaDto>> AtualizarAsync(int id, AtualizarUnidadeDto dto, CancellationToken ct = default)
    {
        // TODO: buscar (404); AlterarNome; dto.Ativo ? Ativar() : Inativar(); Commit.
        // A partir da inativação, ColaboradorService.CriarAsync passa a devolver 422 — sem código extra aqui.
        throw new NotImplementedException();
    }

    public Task<Result<List<UnidadeComColaboradoresDto>>> ListarAsync(CancellationToken ct = default)
    {
        // TODO: ListarComColaboradoresAsync + mapear para DTO
        throw new NotImplementedException();
    }
}
