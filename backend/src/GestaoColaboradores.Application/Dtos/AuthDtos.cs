using GestaoColaboradores.Application.Common;

namespace GestaoColaboradores.Application.Dtos;

public record LoginDto(string Login, [property: NaoNormalizar] string Senha);

public record TokenRespostaDto(string Token, DateTime ExpiraEm);
