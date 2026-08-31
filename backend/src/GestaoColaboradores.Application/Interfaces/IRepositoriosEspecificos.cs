using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Domain.Entidades;

namespace GestaoColaboradores.Application.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObterPorLoginAsync(string login, CancellationToken ct = default);
    /// <param name="ativo">null = todos; true/false = filtro por status.</param>
    Task<(List<Usuario> Itens, int Total)> ListarPaginadoAsync(
        bool? ativo, PaginacaoQuery paginacao, CancellationToken ct = default);
    Task<bool> ExisteLoginAsync(string login, CancellationToken ct = default);
}

public interface IColaboradorRepository : IRepository<Colaborador>
{
    /// <summary>Listagem do enunciado: código, nome e unidade associada (Include).</summary>
    Task<(List<Colaborador> Itens, int Total)> ListarComUnidadePaginadoAsync(
        PaginacaoQuery paginacao, CancellationToken ct = default);

    /// <summary>
    /// Busca rastreada já com a unidade carregada. Necessária quando a atualização pode não
    /// tocar na unidade: sem o Include, montar o DTO de resposta acessaria uma referência nula.
    /// </summary>
    Task<Colaborador?> ObterComUnidadeAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// O vínculo com usuário é 1:1 — o banco tem índice único em UsuarioId. Sem esta
    /// checagem, um usuário já vinculado só falharia no SaveChanges, virando 409 genérico
    /// em vez de uma mensagem que diz qual é o problema.
    /// </summary>
    Task<bool> ExisteParaUsuarioAsync(Guid usuarioId, CancellationToken ct = default);
}

public interface IUnidadeRepository : IRepository<Unidade>
{
    /// <summary>Listagem do enunciado: unidades com seus colaboradores (Include).</summary>
    Task<(List<Unidade> Itens, int Total)> ListarComColaboradoresPaginadoAsync(
        PaginacaoQuery paginacao, CancellationToken ct = default);

    /// <summary>Uma unidade com seus colaboradores, para o endpoint de detalhe.</summary>
    Task<Unidade?> ObterComColaboradoresAsync(Guid id, CancellationToken ct = default);
}
