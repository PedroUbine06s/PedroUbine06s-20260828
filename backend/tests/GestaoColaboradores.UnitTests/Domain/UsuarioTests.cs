using GestaoColaboradores.Domain.Entidades;
using Xunit;

namespace GestaoColaboradores.UnitTests.Domain;

public class UsuarioTests
{
    [Fact]
    public void UsuarioNasceAtivo()
    {
        var usuario = Usuario.Criar("USR-001", "admin", "hash");

        Assert.True(usuario.Ativo);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_SemLogin_Rejeita(string? login)
    {
        Assert.Throws<ArgumentException>(() => Usuario.Criar("USR-001", login!, "hash"));
    }

    [Fact]
    public void Criar_SemSenha_Rejeita()
    {
        Assert.Throws<ArgumentException>(() => Usuario.Criar("USR-001", "admin", ""));
    }

    [Fact]
    public void AlterarSenha_TrocaOHashECarimbaAtualizacao()
    {
        var usuario = Usuario.Criar("USR-001", "admin", "hash-antigo");

        usuario.AlterarSenha("hash-novo");

        Assert.Equal("hash-novo", usuario.SenhaHash);
        Assert.NotNull(usuario.AtualizadoEm);
    }

    [Fact]
    public void AlterarSenha_ComValorVazio_Rejeita()
    {
        var usuario = Usuario.Criar("USR-001", "admin", "hash-antigo");

        Assert.Throws<ArgumentException>(() => usuario.AlterarSenha("  "));
        Assert.Equal("hash-antigo", usuario.SenhaHash);
    }

    [Fact]
    public void Inativar_EDepoisAtivar_VoltaAoEstadoOriginal()
    {
        var usuario = Usuario.Criar("USR-001", "admin", "hash");

        usuario.Inativar();
        Assert.False(usuario.Ativo);

        usuario.Ativar();
        Assert.True(usuario.Ativo);
    }
}
