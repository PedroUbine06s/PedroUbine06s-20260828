using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository genérico (base da herança de repositórios).
/// Especializações herdam daqui e adicionam apenas as queries específicas.
/// </summary>
public class Repository<T>(AppDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext Context = context;
    protected readonly DbSet<T> Set = context.Set<T>();

    public virtual Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public virtual Task<T?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(e => e.Codigo == codigo, ct);

    public virtual async Task AdicionarAsync(T entidade, CancellationToken ct = default) =>
        await Set.AddAsync(entidade, ct);

    public virtual void Remover(T entidade) => Set.Remove(entidade);

    public virtual Task<bool> ExisteCodigoAsync(string codigo, CancellationToken ct = default) =>
        Set.AnyAsync(e => e.Codigo == codigo, ct);
}
