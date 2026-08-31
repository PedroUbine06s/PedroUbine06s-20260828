using GestaoColaboradores.Application.Interfaces;

namespace GestaoColaboradores.Infrastructure.Auth;

/// <summary>Strategy concreta atual. Trocar de algoritmo = nova classe + 1 linha no DI.</summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

    public bool Verificar(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
}
