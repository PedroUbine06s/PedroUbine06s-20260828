using GestaoColaboradores.Application.Common;

namespace GestaoColaboradores.Application.Dtos;

/// <summary>
/// Filtros da listagem de usuários. Cada propriedade nula significa "não filtrar".
///
/// É um record em vez de parâmetros soltos porque os dois filtros são bool?: lado a lado
/// numa assinatura, <c>(false, null)</c> não diz qual é qual, e trocar a ordem compila.
/// Nomeados, a chamada se explica sozinha.
/// </summary>
public record FiltroUsuarios
{
    /// <summary>true = só ativos; false = só inativos; nulo = todos.</summary>
    public bool? Ativo { get; init; }

    /// <summary>
    /// true = só os que ainda não têm colaborador; false = só os já vinculados; nulo = todos.
    /// Um usuário pertence a um único colaborador, então é este filtro que permite ao portal
    /// oferecer apenas usuários elegíveis em vez de esperar o 409.
    /// </summary>
    public bool? SemColaborador { get; init; }
}

// A senha NUNCA aparece em DTO de resposta — nem o hash.
public record UsuarioRespostaDto(Guid Id, string Codigo, string Login, bool Ativo);

/// <summary>O código não é informado: o sistema o gera no formato USR000001.</summary>
public record CriarUsuarioDto(
    string Login,
    [property: NaoNormalizar] string Senha,
    bool Ativo);

/// <summary>
/// Contrato restritivo por design: o enunciado permite atualizar SOMENTE senha e status,
/// então o DTO só oferece esses campos — a API impede o erro em vez de validá-lo depois.
/// Senha nula = não alterar.
/// </summary>
public record AtualizarUsuarioDto([property: NaoNormalizar] string? Senha, bool Ativo);

/// <summary>PATCH: campo ausente ou nulo significa "não alterar este campo".</summary>
public record AtualizarParcialUsuarioDto([property: NaoNormalizar] string? Senha, bool? Ativo);
