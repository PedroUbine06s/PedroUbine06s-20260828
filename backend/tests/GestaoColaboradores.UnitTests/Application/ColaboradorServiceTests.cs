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
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private ColaboradorService CriarService() =>
        new(_colaboradorRepo, _unidadeRepo, _usuarioRepo, _uow);

    private static Unidade UnidadeAtiva() => Unidade.Criar("UNI-001", "Matriz");

    private static Unidade UnidadeInativa()
    {
        var unidade = Unidade.Criar("UNI-002", "Filial Centro");
        unidade.Inativar();
        return unidade;
    }

    private static Usuario UsuarioValido() => Usuario.Criar("USR-001", "maria.silva", "hash");

    private static CriarColaboradorDto Dto(string codigo = "COL-001", string unidade = "UNI-001") =>
        new(codigo, "Maria Silva", unidade, "USR-001");

    // --- Criação -------------------------------------------------------------------

    [Fact]
    public async Task Criar_ComUnidadeInativa_DevolveRegraDeNegocioENaoPersiste()
    {
        _colaboradorRepo.ExisteCodigoAsync("COL-001", Arg.Any<CancellationToken>()).Returns(false);
        _unidadeRepo.ObterPorCodigoAsync("UNI-002", Arg.Any<CancellationToken>()).Returns(UnidadeInativa());

        var resultado = await CriarService().CriarAsync(Dto(unidade: "UNI-002"));

        Assert.Equal(TipoErro.RegraNegocio, resultado.Tipo);
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Criar_ComCodigoDuplicado_DevolveConflito()
    {
        _colaboradorRepo.ExisteCodigoAsync("COL-001", Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await CriarService().CriarAsync(Dto());

        Assert.Equal(TipoErro.Conflito, resultado.Tipo);
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Criar_ComUnidadeInexistente_DevolveNaoEncontrado()
    {
        _colaboradorRepo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _unidadeRepo.ObterPorCodigoAsync("UNI-001", Arg.Any<CancellationToken>()).Returns((Unidade?)null);

        var resultado = await CriarService().CriarAsync(Dto());

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Tipo);
    }

    [Fact]
    public async Task Criar_ComUsuarioInexistente_DevolveNaoEncontrado()
    {
        _colaboradorRepo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _unidadeRepo.ObterPorCodigoAsync("UNI-001", Arg.Any<CancellationToken>()).Returns(UnidadeAtiva());
        _usuarioRepo.ObterPorCodigoAsync("USR-001", Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await CriarService().CriarAsync(Dto());

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Tipo);
    }

    [Fact]
    public async Task Criar_ComDadosValidos_PersisteECommita()
    {
        _colaboradorRepo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _unidadeRepo.ObterPorCodigoAsync("UNI-001", Arg.Any<CancellationToken>()).Returns(UnidadeAtiva());
        _usuarioRepo.ObterPorCodigoAsync("USR-001", Arg.Any<CancellationToken>()).Returns(UsuarioValido());

        var resultado = await CriarService().CriarAsync(Dto());

        Assert.True(resultado.EhSucesso);
        Assert.Equal("Maria Silva", resultado.Valor!.Nome);
        Assert.Equal("UNI-001", resultado.Valor.CodigoUnidade);
        await _colaboradorRepo.Received().AdicionarAsync(Arg.Any<Colaborador>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    // --- Atualização ---------------------------------------------------------------

    [Fact]
    public async Task Atualizar_ParaUnidadeInativa_DevolveRegraDeNegocio()
    {
        var colaborador = Colaborador.Criar("COL-001", "Maria Silva", UnidadeAtiva(), UsuarioValido());
        _colaboradorRepo.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(colaborador);
        _unidadeRepo.ObterPorCodigoAsync("UNI-002", Arg.Any<CancellationToken>()).Returns(UnidadeInativa());

        var resultado = await CriarService().AtualizarAsync(1, new AtualizarColaboradorDto("Novo Nome", "UNI-002"));

        Assert.Equal(TipoErro.RegraNegocio, resultado.Tipo);
        Assert.Equal("Maria Silva", colaborador.Nome); // nada foi alterado
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Atualizar_ComIdInexistente_DevolveNaoEncontrado()
    {
        _colaboradorRepo.ObterPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Colaborador?)null);

        var resultado = await CriarService().AtualizarAsync(99, new AtualizarColaboradorDto("Nome", "UNI-001"));

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Tipo);
    }

    [Fact]
    public async Task AtualizarParcial_ApenasComNome_NaoConsultaUnidades()
    {
        var colaborador = Colaborador.Criar("COL-001", "Maria Silva", UnidadeAtiva(), UsuarioValido());
        _colaboradorRepo.ObterComUnidadeAsync(1, Arg.Any<CancellationToken>()).Returns(colaborador);

        var resultado = await CriarService().AtualizarParcialAsync(1, new AtualizarParcialColaboradorDto("Maria Souza", null));

        Assert.True(resultado.EhSucesso);
        Assert.Equal("Maria Souza", colaborador.Nome);
        await _unidadeRepo.DidNotReceive().ObterPorCodigoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AtualizarParcial_ApenasComUnidade_PreservaONome()
    {
        var colaborador = Colaborador.Criar("COL-001", "Maria Silva", UnidadeAtiva(), UsuarioValido());
        _colaboradorRepo.ObterComUnidadeAsync(1, Arg.Any<CancellationToken>()).Returns(colaborador);
        _unidadeRepo.ObterPorCodigoAsync("UNI-003", Arg.Any<CancellationToken>())
            .Returns(Unidade.Criar("UNI-003", "Filial Sul"));

        await CriarService().AtualizarParcialAsync(1, new AtualizarParcialColaboradorDto(null, "UNI-003"));

        Assert.Equal("Maria Silva", colaborador.Nome);
        Assert.Equal("UNI-003", colaborador.Unidade.Codigo);
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
        var colaborador = Colaborador.Criar("COL-001", "Maria Silva", UnidadeAtiva(), usuario);
        _colaboradorRepo.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(colaborador);
        _usuarioRepo.ObterPorIdAsync(colaborador.UsuarioId, Arg.Any<CancellationToken>()).Returns(usuario);

        var resultado = await CriarService().RemoverAsync(1);

        Assert.True(resultado.EhSucesso);
        Assert.False(usuario.Ativo);
        _colaboradorRepo.Received().Remover(colaborador);
        // As duas alterações num commit só: ou ambas valem, ou nenhuma vale.
        await _uow.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Remover_ComIdInexistente_DevolveNaoEncontrado()
    {
        _colaboradorRepo.ObterPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Colaborador?)null);

        var resultado = await CriarService().RemoverAsync(99);

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Tipo);
        _colaboradorRepo.DidNotReceive().Remover(Arg.Any<Colaborador>());
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }
}
