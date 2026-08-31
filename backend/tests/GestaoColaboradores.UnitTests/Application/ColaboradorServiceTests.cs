using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Application.Services;
using GestaoColaboradores.Domain.Entidades;
using NSubstitute;
using Xunit;

namespace GestaoColaboradores.UnitTests.Application;

/// <summary>
/// A regra central do enunciado no nível do SERVIÇO: aqui se verifica a orquestração
/// (buscar, decidir, não persistir), enquanto os testes de domínio verificam o invariante.
/// </summary>
public class ColaboradorServiceTests
{
    private readonly IColaboradorRepository _colaboradorRepo = Substitute.For<IColaboradorRepository>();
    private readonly IUnidadeRepository _unidadeRepo = Substitute.For<IUnidadeRepository>();
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IGeradorCodigo _gerador = Substitute.For<IGeradorCodigo>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private ColaboradorService CriarService() =>
        new(_colaboradorRepo, _unidadeRepo, _usuarioRepo, _gerador, _uow);

    public ColaboradorServiceTests()
    {
        _gerador.GerarAsync(TipoCodigo.Colaborador, Arg.Any<CancellationToken>()).Returns("COL000001");
    }

    private static Unidade UnidadeAtiva() => Unidade.Criar("UNI000001", "Matriz");

    private static Unidade UnidadeInativa()
    {
        var unidade = Unidade.Criar("UNI000002", "Filial Centro");
        unidade.Inativar();
        return unidade;
    }

    private static Usuario UsuarioValido() => Usuario.Criar("USR000001", "maria.silva", "hash");

    /// <summary>Registra a unidade e o usuário nos mocks e devolve o DTO que os referencia.</summary>
    private CriarColaboradorDto PrepararCriacao(Unidade unidade, Usuario usuario)
    {
        _unidadeRepo.ObterPorIdAsync(unidade.Id, Arg.Any<CancellationToken>()).Returns(unidade);
        _usuarioRepo.ObterPorIdAsync(usuario.Id, Arg.Any<CancellationToken>()).Returns(usuario);

        return new CriarColaboradorDto("Maria Silva", unidade.Id, usuario.Id);
    }

    // --- Criação -------------------------------------------------------------------

    [Fact]
    public async Task Criar_ComUnidadeInativa_DevolveRegraDeNegocioENaoPersiste()
    {
        var dto = PrepararCriacao(UnidadeInativa(), UsuarioValido());

        var resultado = await CriarService().CriarAsync(dto);

        Assert.Equal(TipoErro.RegraNegocio, resultado.Tipo);
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Criar_ComUnidadeInexistente_DevolveNaoEncontrado()
    {
        var inexistente = Guid.CreateVersion7();
        _unidadeRepo.ObterPorIdAsync(inexistente, Arg.Any<CancellationToken>()).Returns((Unidade?)null);

        var resultado = await CriarService().CriarAsync(
            new CriarColaboradorDto("Maria Silva", inexistente, Guid.CreateVersion7()));

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Tipo);
    }

    [Fact]
    public async Task Criar_ComUsuarioInexistente_DevolveNaoEncontrado()
    {
        var unidade = UnidadeAtiva();
        var usuarioInexistente = Guid.CreateVersion7();
        _unidadeRepo.ObterPorIdAsync(unidade.Id, Arg.Any<CancellationToken>()).Returns(unidade);
        _usuarioRepo.ObterPorIdAsync(usuarioInexistente, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await CriarService().CriarAsync(
            new CriarColaboradorDto("Maria Silva", unidade.Id, usuarioInexistente));

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Tipo);
    }

    /// <summary>O código não vem mais do cliente: o serviço pede um ao gerador.</summary>
    [Fact]
    public async Task Criar_ComDadosValidos_UsaOCodigoGeradoEPersiste()
    {
        var unidade = UnidadeAtiva();
        var dto = PrepararCriacao(unidade, UsuarioValido());

        var resultado = await CriarService().CriarAsync(dto);

        Assert.True(resultado.EhSucesso);
        Assert.Equal("COL000001", resultado.Valor!.Codigo);
        Assert.Equal("Maria Silva", resultado.Valor.Nome);
        Assert.Equal(unidade.Id, resultado.Valor.UnidadeId);
        await _gerador.Received().GerarAsync(TipoCodigo.Colaborador, Arg.Any<CancellationToken>());
        await _colaboradorRepo.Received().AdicionarAsync(Arg.Any<Colaborador>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    // --- Atualização ---------------------------------------------------------------

    [Fact]
    public async Task Atualizar_ParaUnidadeInativa_DevolveRegraDeNegocio()
    {
        var colaborador = Colaborador.Criar("COL000001", "Maria Silva", UnidadeAtiva(), UsuarioValido());
        var destinoInativo = UnidadeInativa();
        _colaboradorRepo.ObterPorIdAsync(colaborador.Id, Arg.Any<CancellationToken>()).Returns(colaborador);
        _unidadeRepo.ObterPorIdAsync(destinoInativo.Id, Arg.Any<CancellationToken>()).Returns(destinoInativo);

        var resultado = await CriarService().AtualizarAsync(
            colaborador.Id, new AtualizarColaboradorDto("Novo Nome", destinoInativo.Id));

        Assert.Equal(TipoErro.RegraNegocio, resultado.Tipo);
        Assert.Equal("Maria Silva", colaborador.Nome); // nada foi alterado
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Atualizar_ComIdInexistente_DevolveNaoEncontrado()
    {
        var inexistente = Guid.CreateVersion7();
        _colaboradorRepo.ObterPorIdAsync(inexistente, Arg.Any<CancellationToken>()).Returns((Colaborador?)null);

        var resultado = await CriarService().AtualizarAsync(
            inexistente, new AtualizarColaboradorDto("Nome", Guid.CreateVersion7()));

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Tipo);
    }

    [Fact]
    public async Task AtualizarParcial_ApenasComNome_NaoConsultaUnidades()
    {
        var colaborador = Colaborador.Criar("COL000001", "Maria Silva", UnidadeAtiva(), UsuarioValido());
        _colaboradorRepo.ObterComUnidadeAsync(colaborador.Id, Arg.Any<CancellationToken>()).Returns(colaborador);

        var resultado = await CriarService().AtualizarParcialAsync(
            colaborador.Id, new AtualizarParcialColaboradorDto("Maria Souza", null));

        Assert.True(resultado.EhSucesso);
        Assert.Equal("Maria Souza", colaborador.Nome);
        await _unidadeRepo.DidNotReceive().ObterPorIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarParcial_ApenasComUnidade_PreservaONome()
    {
        var colaborador = Colaborador.Criar("COL000001", "Maria Silva", UnidadeAtiva(), UsuarioValido());
        var destino = Unidade.Criar("UNI000003", "Filial Sul");
        _colaboradorRepo.ObterComUnidadeAsync(colaborador.Id, Arg.Any<CancellationToken>()).Returns(colaborador);
        _unidadeRepo.ObterPorIdAsync(destino.Id, Arg.Any<CancellationToken>()).Returns(destino);

        await CriarService().AtualizarParcialAsync(
            colaborador.Id, new AtualizarParcialColaboradorDto(null, destino.Id));

        Assert.Equal("Maria Silva", colaborador.Nome);
        Assert.Equal(destino.Id, colaborador.UnidadeId);
    }

    // --- Remoção -------------------------------------------------------------------

    /// <summary>
    /// A decisão de domínio registrada no README: remover o colaborador inativa o usuário
    /// vinculado — não o apaga, para preservar o histórico, e não o deixa ativo, para não
    /// sobrar credencial sem dono.
    /// </summary>
    [Fact]
    public async Task Remover_InativaOUsuarioVinculado()
    {
        var usuario = UsuarioValido();
        var colaborador = Colaborador.Criar("COL000001", "Maria Silva", UnidadeAtiva(), usuario);
        _colaboradorRepo.ObterPorIdAsync(colaborador.Id, Arg.Any<CancellationToken>()).Returns(colaborador);
        _usuarioRepo.ObterPorIdAsync(usuario.Id, Arg.Any<CancellationToken>()).Returns(usuario);

        var resultado = await CriarService().RemoverAsync(colaborador.Id);

        Assert.True(resultado.EhSucesso);
        Assert.False(usuario.Ativo);
        _colaboradorRepo.Received().Remover(colaborador);
        // As duas alterações num commit só: ou ambas valem, ou nenhuma vale.
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Remover_ComIdInexistente_DevolveNaoEncontrado()
    {
        var inexistente = Guid.CreateVersion7();
        _colaboradorRepo.ObterPorIdAsync(inexistente, Arg.Any<CancellationToken>()).Returns((Colaborador?)null);

        var resultado = await CriarService().RemoverAsync(inexistente);

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Tipo);
        _colaboradorRepo.DidNotReceive().Remover(Arg.Any<Colaborador>());
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}
