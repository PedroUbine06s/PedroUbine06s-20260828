using System.Net;
using System.Net.Http.Json;
using GestaoColaboradores.Application.Dtos;
using Xunit;

namespace GestaoColaboradores.IntegrationTests;

/// <summary>Sobe a API com um limite baixo para que o limiter possa ser exercitado.</summary>
public class ApiFactoryComLimiteBaixo : ApiFactory
{
    public const int Limite = 3;

    protected override int LoginPorMinuto => Limite;
}

/// <summary>
/// O endpoint de login é o único protegido por rate limiting: é o que um atacante repete
/// milhares de vezes, e cada tentativa custa um BCrypt ao servidor.
/// </summary>
public class RateLimitTests(ApiFactoryComLimiteBaixo factory) : IClassFixture<ApiFactoryComLimiteBaixo>
{
    [Fact]
    public async Task Login_AcimaDoLimite_Responde429ComProblemDetails()
    {
        var client = factory.CreateClient();

        // Credencial errada de propósito: o que se mede é a quantidade de tentativas, não o
        // sucesso delas. A asserção é "o bloqueio acontece dentro do orçamento", e não "na
        // enésima exata", porque a janela é compartilhada por IP com o restante da classe.
        HttpResponseMessage? bloqueada = null;

        for (var i = 0; i <= ApiFactoryComLimiteBaixo.Limite && bloqueada is null; i++)
        {
            var resposta = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto("admin", "errada"));

            if (resposta.StatusCode == HttpStatusCode.TooManyRequests)
                bloqueada = resposta;
            else
                Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        }

        Assert.NotNull(bloqueada);
        Assert.Equal("application/problem+json", bloqueada.Content.Headers.ContentType?.MediaType);
    }

    /// <summary>O limite protege o login sem atrapalhar o resto da API.</summary>
    [Fact]
    public async Task DemaisEndpoints_NaoSaoLimitados()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        for (var i = 0; i < ApiFactoryComLimiteBaixo.Limite + 3; i++)
        {
            var resposta = await client.GetAsync("/api/v1/usuarios");
            Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        }
    }
}
