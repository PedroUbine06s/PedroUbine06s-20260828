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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Default", _db.GetConnectionString());
    }

    /// <summary>Cliente já autenticado com o admin do seed.</summary>
    public async Task<HttpClient> CriarClienteAutenticadoAsync()
    {
        var client = CreateClient();

        var resposta = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto("admin", "admin123"));
        resposta.EnsureSuccessStatusCode();

        var token = (await resposta.Content.ReadFromJsonAsync<TokenRespostaDto>())!.Token;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    public Task InitializeAsync() => _db.StartAsync();

    public new Task DisposeAsync() => _db.DisposeAsync().AsTask();
}
