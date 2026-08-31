using GestaoColaboradores.Application.Interfaces;

namespace GestaoColaboradores.Infrastructure.Persistence;

/// <summary>Delegação fina: o DbContext do EF Core já é um Unit of Work (change tracking + SaveChanges).</summary>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public Task<int> CommitAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
