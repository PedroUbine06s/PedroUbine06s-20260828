using GestaoColaboradores.Application.Interfaces;

namespace GestaoColaboradores.Infrastructure.Auth;

/// <summary>Strategy concreta atual. Trocar de algoritmo = nova classe + 1 linha no DI.</summary>
public class BCryptPasswordHasher : IPasswordHasher
{
    // Calculado uma vez por processo (o hasher é singleton). O conteúdo não importa — importa
    // que seja um hash real, com o mesmo custo de verificação dos demais.
    private static readonly string Descartavel =
        BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

    public string HashDescartavel => Descartavel;

    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

    public bool Verificar(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
}
