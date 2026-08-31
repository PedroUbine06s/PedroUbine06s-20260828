using System.Net.Http.Headers;
using System.Net.Http.Json;
using GestaoColaboradores.Application.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Xunit;

namespace GestaoColaboradores.IntegrationTests;

/// <summary>
/// Sobe um PostgreSQL REAL em contêiner por execução de suíte (Testcontainers) —
/// integração de verdade, sem InMemory provider mascarando comportamento do banco.
/// A aplicação inicia normalmente, então migrations e seed também são exercitados.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("gestao_testes")
        .Build();

    /// <summary>
    /// Limite de login por minuto. A suíte sobe alto para não esbarrar no rate limiter a cada
    /// teste; a classe que verifica o limiter sobrescreve com um valor baixo.
    /// </summary>
    protected virtual int LoginPorMinuto => 1000;

    private string? _token;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", _db.GetConnectionString());
        builder.UseSetting("RateLimit:LoginPorMinuto", LoginPorMinuto.ToString());
    }

    /// <summary>
    /// Cliente já autenticado com o admin do seed. O token é obtido uma vez e reaproveitado:
    /// os testes que não são de autenticação não têm por que refazer login a cada um.
    /// </summary>
    public async Task<HttpClient> CriarClienteAutenticadoAsync()
    {
        var client = CreateClient();

        _token ??= await ObterTokenAsync(client);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

        return client;
    }

    private static async Task<string> ObterTokenAsync(HttpClient client)
    {
        var resposta = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto("admin", "admin123"));
        resposta.EnsureSuccessStatusCode();

        return (await resposta.Content.ReadFromJsonAsync<TokenRespostaDto>())!.Token;
    }

    public Task InitializeAsync() => _db.StartAsync();

    public new Task DisposeAsync() => _db.DisposeAsync().AsTask();
}
