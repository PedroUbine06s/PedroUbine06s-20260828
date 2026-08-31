namespace GestaoColaboradores.Application.Dtos;

public record ColaboradorRespostaDto(int Id, string Codigo, string Nome, string CodigoUnidade, string NomeUnidade);

public record CriarColaboradorDto(string Codigo, string Nome, string CodigoUnidade, string CodigoUsuario);

public record AtualizarColaboradorDto(string Nome, string CodigoUnidade);

/// <summary>PATCH: campo ausente ou nulo significa "não alterar este campo".</summary>
public record AtualizarParcialColaboradorDto(string? Nome, string? CodigoUnidade);
