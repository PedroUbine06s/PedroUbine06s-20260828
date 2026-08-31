using GestaoColaboradores.Domain.Common;

namespace GestaoColaboradores.Application.Interfaces;

/// <summary>Repository genérico — herança restrita a entidades do domínio via constraint.</summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task<T?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task AdicionarAsync(T entidade, CancellationToken ct = default);
    void Remover(T entidade);
    Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default);
}

/// <summary>
/// Unit of Work fino sobre o DbContext — o EF Core já implementa UoW via SaveChanges;
/// esta interface só torna o commit explícito e mockável nos testes.
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
