using GestaoColaboradores.Domain.Entidades;
using Xunit;

namespace GestaoColaboradores.UnitTests.Domain;

/// <summary>
/// O Factory Method promete que a entidade nunca existe em estado inválido.
/// Estes testes cobram essa promessa — inclusive quando a chamada não vem da API.
/// </summary>
public class ColaboradorTests
{
    private static Usuario UsuarioValido() => Usuario.Criar("USR-001", "maria.silva", "hash-qualquer");

    private static Unidade UnidadeAtiva() => Unidade.Criar("UNI-001", "Matriz");

    private static Unidade UnidadeInativa()
    {
        var unidade = Unidade.Criar("UNI-002", "Filial Centro");
        unidade.Inativar();
        return unidade;
    }

    [Fact]
    public void Criar_ComUnidadeAtiva_VinculaUnidadeEUsuario()
    {
        var unidade = UnidadeAtiva();
        var usuario = UsuarioValido();

        var colaborador = Colaborador.Criar("COL-001", "Maria Silva", unidade, usuario);

        Assert.Equal("COL-001", colaborador.Codigo);
        Assert.Same(unidade, colaborador.Unidade);
        Assert.Same(usuario, colaborador.Usuario);
    }

    [Fact]
    public void Criar_ComUnidadeInativa_Rejeita()
    {
        var excecao = Assert.Throws<InvalidOperationException>(
            () => Colaborador.Criar("COL-001", "Maria Silva", UnidadeInativa(), UsuarioValido()));

        Assert.Contains("inativa", excecao.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Criar_SemUnidade_Rejeita()
    {
        Assert.Throws<ArgumentNullException>(
            () => Colaborador.Criar("COL-001", "Maria Silva", null!, UsuarioValido()));
    }

    [Fact]
    public void Criar_SemUsuario_Rejeita()
    {
        Assert.Throws<ArgumentNullException>(
            () => Colaborador.Criar("COL-001", "Maria Silva", UnidadeAtiva(), null!));
    }

    [Fact]
    public void AlterarUnidade_ParaUnidadeInativa_Rejeita()
    {
        var colaborador = Colaborador.Criar("COL-001", "Maria Silva", UnidadeAtiva(), UsuarioValido());

        Assert.Throws<InvalidOperationException>(() => colaborador.AlterarUnidade(UnidadeInativa()));
    }

    [Fact]
    public void AlterarUnidade_MantemUnidadeAnteriorQuandoFalha()
    {
        var unidadeOriginal = UnidadeAtiva();
        var colaborador = Colaborador.Criar("COL-001", "Maria Silva", unidadeOriginal, UsuarioValido());

        Assert.Throws<InvalidOperationException>(() => colaborador.AlterarUnidade(UnidadeInativa()));

        // A entidade não pode ficar meio-alterada: valida tudo antes de mexer em qualquer campo.
        Assert.Same(unidadeOriginal, colaborador.Unidade);
    }

    [Fact]
    public void AlterarUnidade_SincronizaChaveEstrangeira()
    {
        var colaborador = Colaborador.Criar("COL-001", "Maria Silva", UnidadeAtiva(), UsuarioValido());
        var destino = Unidade.Criar("UNI-003", "Filial Sul");

        colaborador.AlterarUnidade(destino);

        Assert.Same(destino, colaborador.Unidade);
        Assert.Equal(destino.Id, colaborador.UnidadeId);
        Assert.NotNull(colaborador.AtualizadoEm);
    }

    [Fact]
    public void AlterarNome_ComNomeVazio_Rejeita()
    {
        var colaborador = Colaborador.Criar("COL-001", "Maria Silva", UnidadeAtiva(), UsuarioValido());

        Assert.Throws<ArgumentException>(() => colaborador.AlterarNome("   "));
        Assert.Equal("Maria Silva", colaborador.Nome);
    }
}
