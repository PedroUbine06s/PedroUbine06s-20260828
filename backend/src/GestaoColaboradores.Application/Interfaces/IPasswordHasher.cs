namespace GestaoColaboradores.Application.Interfaces;

/// <summary>Strategy: algoritmo de hash trocável (BCrypt hoje, Argon2 amanhã) e mockável em teste.</summary>
public interface IPasswordHasher
{
    string Hash(string senha);
    bool Verificar(string senha, string hash);
    string HashDescartavel { get; }
}

public record TokenGerado(string Token, DateTime ExpiraEm);

public interface ITokenService
{
    TokenGerado GerarToken(Domain.Entidades.Usuario usuario);
}
