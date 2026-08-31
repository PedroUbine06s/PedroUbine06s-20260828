using System.Text.Json;
using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using Xunit;

namespace GestaoColaboradores.UnitTests.Application;

/// <summary>
/// Prova os dois lados do contrato de normalização: campos comuns são limpos,
/// campos marcados com [NaoNormalizar] chegam intactos.
/// </summary>
public class NormalizacaoTests
{
    private static readonly JsonSerializerOptions Opcoes =
        Normalizacao.Configurar(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    [Fact]
    public void Desserializacao_DeveCortarEspacosDosCamposComuns()
    {
        var json = """{"login":"  admin  ","senha":"segredo","ativo":true}""";

        var dto = JsonSerializer.Deserialize<CriarUsuarioDto>(json, Opcoes)!;

        Assert.Equal("admin", dto.Login);
    }

    [Fact]
    public void Desserializacao_NaoDeveAlterarSenha()
    {
        // Espaço em senha é legítimo — passphrases são mais fortes, não mais fracas.
        const string senhaComEspacos = "  cavalo bateria grampo  ";
        var json = $$"""{"codigo":"USR-001","login":"admin","senha":"{{senhaComEspacos}}","ativo":true}""";

        var dto = JsonSerializer.Deserialize<CriarUsuarioDto>(json, Opcoes)!;

        Assert.Equal(senhaComEspacos, dto.Senha);
    }

    [Fact]
    public void Desserializacao_DeveProtegerSenhaTambemNoLogin()
    {
        const string senhaComEspacos = " s3nha ";
        var json = $$"""{"login":"  admin  ","senha":"{{senhaComEspacos}}"}""";

        var dto = JsonSerializer.Deserialize<LoginDto>(json, Opcoes)!;

        Assert.Equal("admin", dto.Login);
        Assert.Equal(senhaComEspacos, dto.Senha);
    }
}
