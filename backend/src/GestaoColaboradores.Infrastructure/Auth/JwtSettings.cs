namespace GestaoColaboradores.Infrastructure.Auth;

/// <summary>Options Pattern: configuração tipada, ligada à seção "Jwt" do appsettings/env.</summary>
public class JwtSettings
{
    public const string Secao = "Jwt";

    public string Secret { get; init; } = string.Empty;
    public string Emissor { get; init; } = string.Empty;
    public int ExpiracaoMinutos { get; init; } = 60;
}
