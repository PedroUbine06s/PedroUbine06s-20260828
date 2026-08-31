using GestaoColaboradores.Domain.Entidades;
using Xunit;

namespace GestaoColaboradores.UnitTests.Domain;

/// <summary>A mesma regra, testada também no nível do DOMÍNIO — sem mock nenhum.</summary>
public class UnidadeTests
{
    [Fact]
    public void UnidadeInativa_NaoPodeReceberColaborador()
    {
        var unidade = Unidade.Criar("UNI-001", "Filial Centro");

        unidade.Inativar();

        Assert.False(unidade.PodeReceberColaborador);
    }

    [Fact]
    public void ColaboradorCriar_ComUnidadeInativa_DeveLancarInvalidOperation()
    {
        // TODO: Assert.Throws<InvalidOperationException> em Colaborador.Criar(...)
    }

    // TODO:
    // - Usuario.Criar sem login → ArgumentException
    // - Unidade reativada volta a aceitar colaboradores
}
