using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;
using Xunit;

namespace GestaoColaboradores.IntegrationTests;

/// <summary>
/// Sobe um PostgreSQL REAL em container por execução de suíte (Testcontainers) —
/// integração de verdade, sem InMemory provider mascarando comportamento do banco.
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

    public Task InitializeAsync() => _db.StartAsync();

    public new Task DisposeAsync() => _db.DisposeAsync().AsTask();
}
