namespace GestaoColaboradores.Application.Dtos;

public record ColaboradorRespostaDto(
    Guid Id,
    string Codigo,
    string Nome,
    Guid UnidadeId,
    string CodigoUnidade,
    string NomeUnidade);

/// <summary>
/// O código do colaborador é gerado pelo sistema (COL000001). A unidade e o usuário são
/// referenciados pelo Id, que é o identificador canônico devolvido nas listagens e no
/// header Location.
/// </summary>
public record CriarColaboradorDto(string Nome, Guid UnidadeId, Guid UsuarioId);

public record AtualizarColaboradorDto(string Nome, Guid UnidadeId);

/// <summary>PATCH: campo ausente ou nulo significa "não alterar este campo".</summary>
public record AtualizarParcialColaboradorDto(string? Nome, Guid? UnidadeId);
