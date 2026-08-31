using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.Infrastructure.Persistence.Repositories;

public class UsuarioRepository(AppDbContext context) : Repository<Usuario>(context), IUsuarioRepository
{
    public Task<Usuario?> ObterPorLoginAsync(string login, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(u => u.Login == login, ct);

    public async Task<(List<Usuario> Itens, int Total)> ListarPaginadoAsync(
        FiltroUsuarios filtro, PaginacaoQuery paginacao, CancellationToken ct = default)
    {
        var consulta = Set.AsNoTracking();

        if (filtro.Ativo is not null)
            consulta = consulta.Where(u => u.Ativo == filtro.Ativo.Value);

        // A subconsulta vira EXISTS no SQL: o banco decide o vínculo, sem trazer colaborador
        // algum para a memória. Filtrar isso no cliente exigiria varrer todas as páginas de
        // colaboradores só para montar o conjunto de usuários já ocupados.
        if (filtro.SemColaborador is not null)
        {
            consulta = filtro.SemColaborador.Value
                ? consulta.Where(u => !Context.Colaboradores.Any(c => c.UsuarioId == u.Id))
                : consulta.Where(u => Context.Colaboradores.Any(c => c.UsuarioId == u.Id));
        }

        // A contagem roda sobre o mesmo filtro, antes do recorte: o total precisa refletir
        // o conjunto inteiro, não a página.
        var total = await consulta.CountAsync(ct);

        var itens = await consulta
            .OrderBy(u => u.Login)
            .Skip(paginacao.QuantidadeAPular)
            .Take(paginacao.Tamanho)
            .ToListAsync(ct);

        return (itens, total);
    }


    public Task<bool> ExisteLoginAsync(string login, CancellationToken ct = default) =>
        Set.AnyAsync(e => e.Login == login, ct);

}

public class ColaboradorRepository(AppDbContext context) : Repository<Colaborador>(context), IColaboradorRepository
{
    public async Task<(List<Colaborador> Itens, int Total)> ListarComUnidadePaginadoAsync(
        PaginacaoQuery paginacao, CancellationToken ct = default)
    {
        var total = await Set.CountAsync(ct);

        var itens = await Set.AsNoTracking()
            .Include(c => c.Unidade)
            .OrderBy(c => c.Nome)
            .Skip(paginacao.QuantidadeAPular)
            .Take(paginacao.Tamanho)
            .ToListAsync(ct);

        return (itens, total);
    }

    // Sem AsNoTracking, ao contrário da listagem: quem busca por id vai alterar.
    public Task<Colaborador?> ObterComUnidadeAsync(Guid id, CancellationToken ct = default) =>
        Set.Include(c => c.Unidade).FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<bool> ExisteParaUsuarioAsync(Guid usuarioId, CancellationToken ct = default) =>
        Set.AnyAsync(c => c.UsuarioId == usuarioId, ct);
}

public class UnidadeRepository(AppDbContext context) : Repository<Unidade>(context), IUnidadeRepository
{
    public Task<Unidade?> ObterComColaboradoresAsync(Guid id, CancellationToken ct = default) =>
        Set.Include(u => u.Colaboradores).FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<(List<Unidade> Itens, int Total)> ListarComColaboradoresPaginadoAsync(
        PaginacaoQuery paginacao, CancellationToken ct = default)
    {
        var total = await Set.CountAsync(ct);

        // O recorte é aplicado às unidades; os colaboradores de cada uma vêm inteiros, porque
        // são um detalhe da unidade e não uma coleção independente.
        var itens = await Set.AsNoTracking()
            .Include(u => u.Colaboradores)
            .OrderBy(u => u.Nome)
            .Skip(paginacao.QuantidadeAPular)
            .Take(paginacao.Tamanho)
            .ToListAsync(ct);

        return (itens, total);
    }
}
