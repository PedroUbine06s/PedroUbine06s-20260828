using System.Net;
using System.Net.Http.Json;
using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using Xunit;

namespace GestaoColaboradores.IntegrationTests;

/// <summary>
/// Fluxos ponta a ponta: HTTP real, pipeline completo de autenticação e validação,
/// e PostgreSQL de verdade — inclusive os índices únicos e as sequences de código, que o
/// InMemory provider não teria. Os testes compartilham o banco semeado, então cada um cria
/// os próprios registros e resolve os Ids por consulta, sem depender de valores fixos.
/// </summary>
public class FluxosTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static async Task<UnidadeComColaboradoresDto> UnidadeDoSeedAsync(HttpClient client, bool ativa)
    {
        var pagina = await client.GetFromJsonAsync<PaginaDto<UnidadeComColaboradoresDto>>("/api/v1/unidades");
        return pagina!.Itens.First(u => u.Ativo == ativa);
    }

    private static async Task<UsuarioRespostaDto> CriarUsuarioAsync(HttpClient client, string login)
    {
        var resposta = await client.PostAsJsonAsync("/api/v1/usuarios",
            new CriarUsuarioDto(login, "senha123", true));
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<UsuarioRespostaDto>())!;
    }

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

    // --- Códigos gerados pelo sistema ------------------------------------------------

    /// <summary>O código não é entrada do cliente: o sistema numera no formato do prefixo.</summary>
    [Fact]
    public async Task Criar_GeraOCodigoNoFormatoDoSistema()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var usuario = await CriarUsuarioAsync(client, "codigo.gerado");

        Assert.Matches(@"^USR\d{6}$", usuario.Codigo);
    }

    /// <summary>Duas criações seguidas nunca recebem o mesmo número — é o papel da sequence.</summary>
    [Fact]
    public async Task Criar_EmSequencia_NuncaRepeteOCodigo()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var primeiro = await CriarUsuarioAsync(client, "sequencia.um");
        var segundo = await CriarUsuarioAsync(client, "sequencia.dois");

        Assert.NotEqual(primeiro.Codigo, segundo.Codigo);
    }

    [Fact]
    public async Task Criar_AtribuiIdentificadorUnicoNaoSequencial()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var usuario = await CriarUsuarioAsync(client, "identificador.uuid");

        Assert.NotEqual(Guid.Empty, usuario.Id);
    }

    // --- Seed e listagens ----------------------------------------------------------

    /// <summary>Requisito: listar unidades COM os colaboradores relacionados.</summary>
    [Fact]
    public async Task ListarUnidades_TrazOsColaboradoresAninhados()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var matriz = await UnidadeDoSeedAsync(client, ativa: true);

        Assert.NotEmpty(matriz.Colaboradores);
        Assert.All(matriz.Colaboradores, c => Assert.Equal(matriz.Id, c.UnidadeId));
    }

    /// <summary>Inativar não desvincula quem já estava na unidade.</summary>
    [Fact]
    public async Task UnidadeInativaDoSeed_ContinuaComSeusColaboradores()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var filial = await UnidadeDoSeedAsync(client, ativa: false);

        Assert.False(filial.Ativo);
        Assert.NotEmpty(filial.Colaboradores);
    }

    /// <summary>Requisito: consulta de usuários apenas por status.</summary>
    [Fact]
    public async Task ListarUsuarios_ComFiltroDeStatus_TrazApenasOsInativos()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var pagina = await client.GetFromJsonAsync<PaginaDto<UsuarioRespostaDto>>("/api/v1/usuarios?ativo=false");

        Assert.NotEmpty(pagina!.Itens);
        Assert.All(pagina.Itens, u => Assert.False(u.Ativo));
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

        var respostaUnidade = await client.PostAsJsonAsync("/api/v1/unidades", new CriarUnidadeDto("Filial do Fluxo"));
        Assert.Equal(HttpStatusCode.Created, respostaUnidade.StatusCode);
        Assert.NotNull(respostaUnidade.Headers.Location);
        var unidade = (await respostaUnidade.Content.ReadFromJsonAsync<UnidadeRespostaDto>())!;

        var usuario = await CriarUsuarioAsync(client, "fluxo.teste");

        var respostaColaborador = await client.PostAsJsonAsync("/api/v1/colaboradores",
            new CriarColaboradorDto("Colaborador do Fluxo", unidade.Id, usuario.Id));
        Assert.Equal(HttpStatusCode.Created, respostaColaborador.StatusCode);

        // O Location precisa apontar para o próprio recurso, não para a coleção.
        var criado = await client.GetFromJsonAsync<ColaboradorRespostaDto>(
            respostaColaborador.Headers.Location!.PathAndQuery);
        Assert.Equal("Colaborador do Fluxo", criado!.Nome);
        Assert.Equal(unidade.Id, criado.UnidadeId);
        Assert.Matches(@"^COL\d{6}$", criado.Codigo);
    }

    // --- Regras de negócio ----------------------------------------------------------

    /// <summary>A regra central do enunciado, ponta a ponta.</summary>
    [Fact]
    public async Task CriarColaborador_EmUnidadeInativa_Responde422()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var filial = await UnidadeDoSeedAsync(client, ativa: false);
        var usuario = await CriarUsuarioAsync(client, "usuario.422");

        var resposta = await client.PostAsJsonAsync("/api/v1/colaboradores",
            new CriarColaboradorDto("Deve Falhar", filial.Id, usuario.Id));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("inativa", corpo, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Só o índice único do banco garante isso — por isso o teste usa Postgres real.</summary>
    [Fact]
    public async Task CriarUsuario_ComLoginDuplicado_Responde409()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var resposta = await client.PostAsJsonAsync("/api/v1/usuarios",
            new CriarUsuarioDto("admin", "senha123", true));

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    [Fact]
    public async Task CriarColaborador_ComUnidadeInexistente_Responde404()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var usuario = await CriarUsuarioAsync(client, "sem.unidade");

        var resposta = await client.PostAsJsonAsync("/api/v1/colaboradores",
            new CriarColaboradorDto("Sem Unidade", Guid.CreateVersion7(), usuario.Id));

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    // --- Validação e verbos ---------------------------------------------------------

    [Fact]
    public async Task CriarUsuario_ComSenhaCurta_Responde400()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var resposta = await client.PostAsJsonAsync("/api/v1/usuarios",
            new CriarUsuarioDto("senha.curta", "123", true));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task ObterUsuario_ComIdInexistente_Responde404()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var resposta = await client.GetAsync($"/api/v1/usuarios/{Guid.CreateVersion7()}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    /// <summary>PUT exige a representação inteira; PATCH aceita só o que muda.</summary>
    [Fact]
    public async Task InativarUnidade_ExigeNomeNoPutMasNaoNoPatch()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var criada = await client.PostAsJsonAsync("/api/v1/unidades", new CriarUnidadeDto("Filial dos Verbos"));
        var unidade = (await criada.Content.ReadFromJsonAsync<UnidadeRespostaDto>())!;

        var put = await client.PutAsJsonAsync($"/api/v1/unidades/{unidade.Id}", new { ativo = false });
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
        var usuario = await CriarUsuarioAsync(client, "patch.vazio");

        var resposta = await client.PatchAsJsonAsync($"/api/v1/usuarios/{usuario.Id}", new { });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    /// <summary>Decisão de domínio: remover o colaborador inativa o usuário vinculado.</summary>
    [Fact]
    public async Task RemoverColaborador_InativaOUsuarioVinculado()
    {
        var client = await factory.CriarClienteAutenticadoAsync();

        var criada = await client.PostAsJsonAsync("/api/v1/unidades", new CriarUnidadeDto("Filial da Remoção"));
        var unidade = (await criada.Content.ReadFromJsonAsync<UnidadeRespostaDto>())!;
        var usuario = await CriarUsuarioAsync(client, "sera.removido");

        var respostaColaborador = await client.PostAsJsonAsync("/api/v1/colaboradores",
            new CriarColaboradorDto("Será Removido", unidade.Id, usuario.Id));
        var colaborador = (await respostaColaborador.Content.ReadFromJsonAsync<ColaboradorRespostaDto>())!;

        var remocao = await client.DeleteAsync($"/api/v1/colaboradores/{colaborador.Id}");
        Assert.Equal(HttpStatusCode.NoContent, remocao.StatusCode);

        var usuarioDepois = await client.GetFromJsonAsync<UsuarioRespostaDto>($"/api/v1/usuarios/{usuario.Id}");
        Assert.False(usuarioDepois!.Ativo);

        var colaboradorDepois = await client.GetAsync($"/api/v1/colaboradores/{colaborador.Id}");
        Assert.Equal(HttpStatusCode.NotFound, colaboradorDepois.StatusCode);
    }

    /// <summary>
    /// O vínculo colaborador-usuário é 1:1. Sem esta checagem, o índice único do banco
    /// estouraria no SaveChanges e o cliente receberia 500 em vez de 409.
    /// </summary>
    [Fact]
    public async Task CriarColaborador_ComUsuarioJaVinculado_Responde409()
    {
        var client = await factory.CriarClienteAutenticadoAsync();
        var unidade = await UnidadeDoSeedAsync(client, ativa: true);
        var usuario = await CriarUsuarioAsync(client, "usuario.reutilizado");

        var primeiro = await client.PostAsJsonAsync("/api/v1/colaboradores",
            new CriarColaboradorDto("Primeiro", unidade.Id, usuario.Id));
        Assert.Equal(HttpStatusCode.Created, primeiro.StatusCode);

        var segundo = await client.PostAsJsonAsync("/api/v1/colaboradores",
            new CriarColaboradorDto("Segundo", unidade.Id, usuario.Id));

        Assert.Equal(HttpStatusCode.Conflict, segundo.StatusCode);
    }

    /// <summary>
    /// Exercita o BCrypt de verdade no caminho "login não existe" — o único lugar onde o
    /// hash descartável é usado. Os testes de unidade mockam o hasher e não alcançam isto.
    /// </summary>
    [Fact]
    public async Task Login_ComLoginInexistente_Responde401SemErroDeFormato()
    {
        var client = factory.CreateClient();

        var resposta = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginDto("nao.existe.em.lugar.nenhum", "qualquerSenha123"));

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);

        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("inválidos", corpo, StringComparison.OrdinalIgnoreCase);
    }
}
