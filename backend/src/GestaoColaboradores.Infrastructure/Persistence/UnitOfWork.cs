using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GestaoColaboradores.Infrastructure.Persistence;

/// <summary>
/// Delegação fina: o DbContext do EF Core já é um Unit of Work (change tracking + SaveChanges).
///
/// O único acréscimo é traduzir a violação de unicidade do PostgreSQL em uma exceção da
/// aplicação. Este é o lugar certo para isso: é a fronteira onde o dialeto do banco termina.
/// </summary>
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    /// <summary>SQLSTATE 23505 — unique_violation.</summary>
    private const string ViolacaoDeUnicidade = "23505";

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        try
        {
            return await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConflitoDeConcorrenciaException(
                "O registro foi alterado por outra pessoa. Recarregue e tente novamente.", ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: ViolacaoDeUnicidade } pg)
        {
            throw new ConflitoDePersistenciaException(
                $"O registro viola uma restrição de unicidade ({pg.ConstraintName}).", ex);
        }
    }
}
