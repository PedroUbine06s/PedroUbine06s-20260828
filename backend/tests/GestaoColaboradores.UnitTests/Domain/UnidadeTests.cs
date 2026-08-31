using GestaoColaboradores.Domain.Entidades;
using Xunit;

namespace GestaoColaboradores.UnitTests.Domain;

/// <summary>
/// A regra central do enunciado testada no nível do DOMÍNIO — sem mock, sem banco.
/// É o teste mais barato e mais resistente a refatoração que existe no projeto.
/// </summary>
public class UnidadeTests
{
    [Fact]
    public void UnidadeNasceAtiva()
    {
        var unidade = Unidade.Criar("UNI-001", "Matriz");

        Assert.True(unidade.Ativo);
        Assert.True(unidade.PodeReceberColaborador);
    }

    [Fact]
    public void UnidadeInativa_NaoPodeReceberColaborador()
    {
        var unidade = Unidade.Criar("UNI-001", "Filial Centro");

        unidade.Inativar();

        Assert.False(unidade.PodeReceberColaborador);
    }

    [Fact]
    public void UnidadeReativada_VoltaAAceitarColaboradores()
    {
        var unidade = Unidade.Criar("UNI-001", "Filial Centro");
        unidade.Inativar();

        unidade.Ativar();

        Assert.True(unidade.PodeReceberColaborador);
    }

    [Fact]
    public void AlterarNome_CarimbaDataDeAtualizacao()
    {
        var unidade = Unidade.Criar("UNI-001", "Matriz");
        Assert.Null(unidade.AtualizadoEm);

        unidade.AlterarNome("Matriz Nova");

        Assert.Equal("Matriz Nova", unidade.Nome);
        Assert.NotNull(unidade.AtualizadoEm);
    }

    [Fact]
    public void Inativar_CarimbaDataDeAtualizacao()
    {
        var unidade = Unidade.Criar("UNI-001", "Matriz");

        unidade.Inativar();

        Assert.NotNull(unidade.AtualizadoEm);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_SemCodigo_Rejeita(string? codigo)
    {
        Assert.Throws<ArgumentException>(() => Unidade.Criar(codigo!, "Matriz"));
    }

    [Fact]
    public void Criar_ComNomeAcimaDoLimite_Rejeita()
    {
        var nomeLongo = new string('a', Unidade.TamanhoMaximoNome + 1);

        Assert.Throws<ArgumentException>(() => Unidade.Criar("UNI-001", nomeLongo));
    }

    [Fact]
    public void Criar_RemoveEspacosDasPontas()
    {
        var unidade = Unidade.Criar("  UNI-001  ", "  Matriz  ");

        Assert.Equal("UNI-001", unidade.Codigo);
        Assert.Equal("Matriz", unidade.Nome);
    }
}
