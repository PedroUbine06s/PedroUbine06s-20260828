namespace GestaoColaboradores.Application.Dtos;

public record ColaboradorRespostaDto(int Id, string Codigo, string Nome, string CodigoUnidade, string NomeUnidade);

public record CriarColaboradorDto(string Codigo, string Nome, string CodigoUnidade, string CodigoUsuario);

public record AtualizarColaboradorDto(string Nome, string CodigoUnidade);
