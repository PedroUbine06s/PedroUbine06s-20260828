using GestaoColaboradores.Domain.Entidades;

namespace GestaoColaboradores.Application.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObterPorLoginAsync(string login, CancellationToken ct = default);
    Task<List<Usuario>> ListarPorStatusAsync(bool ativo, CancellationToken ct = default);
    Task<bool> ExisteLoginAsync(string login, CancellationToken ct = default);
}

public interface IColaboradorRepository : IRepository<Colaborador>
{
    /// <summary>Listagem do enunciado: código, nome e unidade associada (Include).</summary>
    Task<List<Colaborador>> ListarComUnidadeAsync(CancellationToken ct = default);

    /// <summary>
    /// Busca rastreada já com a unidade carregada. Necessária quando a atualização pode não
    /// tocar na unidade: sem o Include, montar o DTO de resposta acessaria uma referência nula.
    /// </summary>
    Task<Colaborador?> ObterComUnidadeAsync(Guid id, CancellationToken ct = default);
}

public interface IUnidadeRepository : IRepository<Unidade>
{
    /// <summary>Listagem do enunciado: unidades com seus colaboradores (Include).</summary>
    Task<List<Unidade>> ListarComColaboradoresAsync(CancellationToken ct = default);

    /// <summary>Uma unidade com seus colaboradores, para o endpoint de detalhe.</summary>
    Task<Unidade?> ObterComColaboradoresAsync(Guid id, CancellationToken ct = default);
}
