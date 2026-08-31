namespace GestaoColaboradores.Application.Dtos;

public record UnidadeRespostaDto(Guid Id, string Codigo, string Nome, bool Ativo);

public record UnidadeComColaboradoresDto(
    Guid Id,
    string Codigo,
    string Nome,
    bool Ativo,
    List<ColaboradorRespostaDto> Colaboradores);

/// <summary>O código não é informado: o sistema o gera no formato UNI000001.</summary>
public record CriarUnidadeDto(string Nome);

public record AtualizarUnidadeDto(string Nome, bool Ativo);

/// <summary>PATCH: campo ausente ou nulo significa "não alterar este campo".</summary>
public record AtualizarParcialUnidadeDto(string? Nome, bool? Ativo);
