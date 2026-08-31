using System.Net;
using Xunit;

namespace GestaoColaboradores.IntegrationTests;

public class FluxosTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Endpoints_SemToken_DevemResponder401()
    {
        var resposta = await _client.GetAsync("/api/v1/colaboradores");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    // TODO (fluxos que valem a suíte):
    // 1. Login com admin do seed → 200 + token
    //    (helper: método que loga e devolve HttpClient com Authorization já setado)
    // 2. Fluxo completo: criar unidade → criar usuário → criar colaborador → listar (201/201/201/200)
    // 3. Criar colaborador em unidade INATIVA → 422
    // 4. Criar usuário com código duplicado → 409
}
