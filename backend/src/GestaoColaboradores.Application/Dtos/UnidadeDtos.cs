namespace GestaoColaboradores.Application.Dtos;

public record UnidadeRespostaDto(int Id, string Codigo, string Nome, bool Ativo);

public record UnidadeComColaboradoresDto(int Id, string Codigo, string Nome, bool Ativo, List<ColaboradorRespostaDto> Colaboradores);

public record CriarUnidadeDto(string Codigo, string Nome);

public record AtualizarUnidadeDto(string Nome, bool Ativo);

/// <summary>PATCH: campo ausente ou nulo significa "não alterar este campo".</summary>
public record AtualizarParcialUnidadeDto(string? Nome, bool? Ativo);
