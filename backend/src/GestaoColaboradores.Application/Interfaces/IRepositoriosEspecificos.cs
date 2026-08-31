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
}

public interface IUnidadeRepository : IRepository<Unidade>
{
    /// <summary>Listagem do enunciado: unidades com seus colaboradores (Include).</summary>
    Task<List<Unidade>> ListarComColaboradoresAsync(CancellationToken ct = default);
}
