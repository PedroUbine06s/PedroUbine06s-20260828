namespace GestaoColaboradores.Infrastructure.Auth;

/// <summary>Options Pattern: configuração tipada, ligada à seção "Jwt" do appsettings/env.</summary>
public class JwtSettings
{
    public const string Secao = "Jwt";


    public const int TamanhoMinimoSecret = 32;


    public const string SecretDeDesenvolvimento = "chave-de-desenvolvimento-local-min-32-caracteres!!";

    public string Secret { get; init; } = string.Empty;
    public string Emissor { get; init; } = string.Empty;
    public int ExpiracaoMinutos { get; init; } = 60;


    public void Validar(bool ambienteDeDesenvolvimento)
    {
        if (string.IsNullOrWhiteSpace(Secret) || Secret.Length < TamanhoMinimoSecret)
            throw new InvalidOperationException(
                $"Jwt:Secret precisa ter ao menos {TamanhoMinimoSecret} caracteres. " +
                "Defina a variável de ambiente Jwt__Secret.");

        if (!ambienteDeDesenvolvimento && Secret == SecretDeDesenvolvimento)
            throw new InvalidOperationException(
                "Jwt:Secret está com o valor de desenvolvimento, que é público por estar versionado. " +
                "Defina a variável de ambiente Jwt__Secret com um segredo próprio.");

        if (string.IsNullOrWhiteSpace(Emissor))
            throw new InvalidOperationException("Jwt:Emissor é obrigatório.");
    }
}
