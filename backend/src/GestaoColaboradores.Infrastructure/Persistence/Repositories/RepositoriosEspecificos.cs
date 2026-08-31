using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.Infrastructure.Persistence.Repositories;

public class UsuarioRepository(AppDbContext context) : Repository<Usuario>(context), IUsuarioRepository
{
    public Task<Usuario?> ObterPorLoginAsync(string login, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(u => u.Login == login, ct);

    public Task<List<Usuario>> ListarPorStatusAsync(bool ativo, CancellationToken ct = default)
    {
        // TODO: filtrar por Ativo == ativo, AsNoTracking
        throw new NotImplementedException();
    }

    public Task<bool> ExisteLoginAsync(string login, CancellationToken ct = default) =>
        Set.AnyAsync(e => e.Login == login, ct);
    
}

public class ColaboradorRepository(AppDbContext context) : Repository<Colaborador>(context), IColaboradorRepository
{
    public Task<List<Colaborador>> ListarComUnidadeAsync(CancellationToken ct = default) =>
        Set.AsNoTracking().Include(c => c.Unidade).OrderBy(c => c.Nome).ToListAsync(ct);
}

public class UnidadeRepository(AppDbContext context) : Repository<Unidade>(context), IUnidadeRepository
{
    public Task<List<Unidade>> ListarComColaboradoresAsync(CancellationToken ct = default)
    {
        // TODO: Include(u => u.Colaboradores), AsNoTracking, ordenar por nome
        throw new NotImplementedException();
    }
}
