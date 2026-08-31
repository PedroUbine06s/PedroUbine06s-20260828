using GestaoColaboradores.Application.Common;

namespace GestaoColaboradores.Application.Dtos;

// A senha NUNCA aparece em DTO de resposta — nem o hash.
public record UsuarioRespostaDto(int Id, string Codigo, string Login, bool Ativo);

public record CriarUsuarioDto(
    string Codigo,
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
