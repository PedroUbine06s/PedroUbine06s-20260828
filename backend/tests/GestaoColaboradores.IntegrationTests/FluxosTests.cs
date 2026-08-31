using System.Net;
using System.Net.Http.Json;
using GestaoColaboradores.Application.Dtos;
using Xunit;

namespace GestaoColaboradores.IntegrationTests;

/// <summary>
/// Fluxos ponta a ponta: HTTP real, pipeline completo de autenticação e validação,
/// e PostgreSQL de verdade — inclusive os índices únicos, que o InMemory não teria.
/// Os testes compartilham o banco semeado, então cada um usa códigos próprios.
/// </summary>
public class FluxosTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // --- Autenticação --------------------------------------------------------------

    [Fact]
    public async Task Endpoints_SemToken_DevemResponder401()
    {
        var client = factory.CreateClient();

        var resposta = await client.GetAsync("/api/v1/colaboradores");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_ComCredenciaisDoSeed_DevolveToken()
    {
        var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto("admin", "admin123"));

        resposta.EnsureSuccessStatusCode();
        var token = await resposta.Content.ReadFromJsonAsync<TokenRespostaDto>();
        Assert.False(string.IsNullOrWhiteSpace(token!.Token));
        Assert.True(token.ExpiraEm > DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_ComSenhaErrada_Responde401()
    {
        var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto("admin", "errada"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    /// <summary>Usuário inativo do seed: credencial correta, acesso negado mesmo assim.</summary>
    [Fact]
    public async Task Login_ComUsuarioInativo_Responde401()
    {
        var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync("/api/v1/auth/login", new LoginDto("carlos.lima", "senha123"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    // --- Seed e listagens ----------------------------------------------------------

    /// <summary>Requisito: listar unidades COM os colaboradores relacionados.</summary>
    [Fact]
    public async Task ListarUnidades_TrazOsColaboradoresAninhados()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var unidades = await client.GetFromJsonAsync<List<UnidadeComColaboradoresDto>>("/api/v1/unidades");

        var matriz = Assert.Single(unidades!, u => u.Codigo == "UNI-001");
        Assert.NotEmpty(matriz.Colaboradores);
    }

    /// <summary>Inativar não desvincula quem já estava na unidade.</summary>
    [Fact]
    public async Task UnidadeInativaDoSeed_ContinuaComSeusColaboradores()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var unidades = await client.GetFromJsonAsync<List<UnidadeComColaboradoresDto>>("/api/v1/unidades");

        var filial = Assert.Single(unidades!, u => u.Codigo == "UNI-002");
        Assert.False(filial.Ativo);
        Assert.NotEmpty(filial.Colaboradores);
    }

    /// <summary>Requisito: consulta de usuários apenas por status.</summary>
    [Fact]
    public async Task ListarUsuarios_ComFiltroDeStatus_TrazApenasOsInativos()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var usuarios = await client.GetFromJsonAsync<List<UsuarioRespostaDto>>("/api/v1/usuarios?ativo=false");

        Assert.NotEmpty(usuarios!);
        Assert.All(usuarios!, u => Assert.False(u.Ativo));
    }

    [Fact]
    public async Task ListarUsuarios_NuncaExpoeSenha()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var json = await client.GetStringAsync("/api/v1/usuarios");

        Assert.DoesNotContain("senha", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("hash", json, StringComparison.OrdinalIgnoreCase);
    }

    // --- Fluxo completo de cadastro -------------------------------------------------

    [Fact]
    public async Task FluxoCompleto_CriarUnidadeUsuarioEColaborador()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var respostaUnidade = await client.PostAsJsonAsync("/api/v1/unidades",
            new CriarUnidadeDto("UNI-F01", "Filial do Fluxo"));
        Assert.Equal(HttpStatusCode.Created, respostaUnidade.StatusCode);
        Assert.NotNull(respostaUnidade.Headers.Location);

        var respostaUsuario = await client.PostAsJsonAsync("/api/v1/usuarios",
            new CriarUsuarioDto("USR-F01", "fluxo.teste", "senha123", true));
        Assert.Equal(HttpStatusCode.Created, respostaUsuario.StatusCode);

        var respostaColaborador = await client.PostAsJsonAsync("/api/v1/colaboradores",
            new CriarColaboradorDto("COL-F01", "Colaborador do Fluxo", "UNI-F01", "USR-F01"));
        Assert.Equal(HttpStatusCode.Created, respostaColaborador.StatusCode);

        // O Location precisa apontar para o próprio recurso, não para a coleção.
        var criado = await client.GetFromJsonAsync<ColaboradorRespostaDto>(
            respostaColaborador.Headers.Location!.PathAndQuery);
        Assert.Equal("COL-F01", criado!.Codigo);
        Assert.Equal("UNI-F01", criado.CodigoUnidade);
    }

    // --- Regras de negócio ----------------------------------------------------------

    /// <summary>A regra central do enunciado, ponta a ponta.</summary>
    [Fact]
    public async Task CriarColaborador_EmUnidadeInativa_Responde422()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        await client.PostAsJsonAsync("/api/v1/usuarios",
            new CriarUsuarioDto("USR-F02", "usuario.422", "senha123", true));

        var resposta = await client.PostAsJsonAsync("/api/v1/colaboradores",
            new CriarColaboradorDto("COL-F02", "Deve Falhar", "UNI-002", "USR-F02"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("inativa", corpo, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Só o índice único do banco garante isso — por isso o teste usa Postgres real.</summary>
    [Fact]
    public async Task CriarUsuario_ComCodigoDuplicado_Responde409()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var resposta = await client.PostAsJsonAsync("/api/v1/usuarios",
            new CriarUsuarioDto("USR-001", "outro.login", "senha123", true));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task CriarUsuario_ComLoginDuplicado_Responde409()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var resposta = await client.PostAsJsonAsync("/api/v1/usuarios",
            new CriarUsuarioDto("USR-F03", "admin", "senha123", true));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    // --- Validação e verbos ---------------------------------------------------------

    [Fact]
    public async Task CriarUsuario_ComSenhaCurta_Responde400()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var resposta = await client.PostAsJsonAsync("/api/v1/usuarios",
            new CriarUsuarioDto("USR-F04", "senha.curta", "123", true));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task ObterUsuario_ComIdInexistente_Responde404()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var resposta = await client.GetAsync("/api/v1/usuarios/999999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    /// <summary>PUT exige a representação inteira; PATCH aceita só o que muda.</summary>
    [Fact]
    public async Task InativarUnidade_ExigeNomeNoPutMasNaoNoPatch()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var criada = await client.PostAsJsonAsync("/api/v1/unidades",
            new CriarUnidadeDto("UNI-F02", "Filial dos Verbos"));
        var unidade = await criada.Content.ReadFromJsonAsync<UnidadeRespostaDto>();

        var put = await client.PutAsJsonAsync($"/api/v1/unidades/{unidade!.Id}", new { ativo = false });
        Assert.Equal(HttpStatusCode.BadRequest, put.StatusCode);

        var patch = await client.PatchAsJsonAsync($"/api/v1/unidades/{unidade.Id}", new { ativo = false });
        Assert.Equal(HttpStatusCode.OK, patch.StatusCode);

        var atualizada = await patch.Content.ReadFromJsonAsync<UnidadeRespostaDto>();
        Assert.False(atualizada!.Ativo);
        Assert.Equal("Filial dos Verbos", atualizada.Nome);
    }

    [Fact]
    public async Task Patch_SemNenhumCampo_Responde400()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var resposta = await client.PatchAsJsonAsync("/api/v1/usuarios/1", new { });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    /// <summary>Decisão de domínio: remover o colaborador inativa o usuário vinculado.</summary>
    [Fact]
    public async Task RemoverColaborador_InativaOUsuarioVinculado()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        await client.PostAsJsonAsync("/api/v1/unidades", new CriarUnidadeDto("UNI-F03", "Filial da Remoção"));
        var respostaUsuario = await client.PostAsJsonAsync("/api/v1/usuarios",
            new CriarUsuarioDto("USR-F05", "sera.removido", "senha123", true));
        var usuario = await respostaUsuario.Content.ReadFromJsonAsync<UsuarioRespostaDto>();

        var respostaColaborador = await client.PostAsJsonAsync("/api/v1/colaboradores",
            new CriarColaboradorDto("COL-F03", "Será Removido", "UNI-F03", "USR-F05"));
        var colaborador = await respostaColaborador.Content.ReadFromJsonAsync<ColaboradorRespostaDto>();

        var remocao = await client.DeleteAsync($"/api/v1/colaboradores/{colaborador!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, remocao.StatusCode);

        var usuarioDepois = await client.GetFromJsonAsync<UsuarioRespostaDto>($"/api/v1/usuarios/{usuario!.Id}");
        Assert.False(usuarioDepois!.Ativo);

        var colaboradorDepois = await client.GetAsync($"/api/v1/colaboradores/{colaborador.Id}");
        Assert.Equal(HttpStatusCode.NotFound, colaboradorDepois.StatusCode);
    }
}
