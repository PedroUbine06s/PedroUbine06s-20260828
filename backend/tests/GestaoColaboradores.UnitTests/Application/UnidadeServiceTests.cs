using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Application.Services;
using GestaoColaboradores.Domain.Entidades;
using NSubstitute;
using Xunit;

namespace GestaoColaboradores.UnitTests.Application;

public class UnidadeServiceTests
{
    private readonly IUnidadeRepository _unidadeRepo = Substitute.For<IUnidadeRepository>();
    private readonly IGeradorCodigo _gerador = Substitute.For<IGeradorCodigo>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private UnidadeService CriarService() => new(_unidadeRepo, _gerador, _uow);

    public UnidadeServiceTests()
    {
        _gerador.GerarAsync(TipoCodigo.Unidade, Arg.Any<CancellationToken>()).Returns("UNI000001");
    }

    /// <summary>O código não vem mais do cliente: o serviço pede um ao gerador.</summary>
    [Fact]
    public async Task Criar_ComDadosValidos_UsaOCodigoGeradoEPersiste()
    {
        var resultado = await CriarService().CriarAsync(new CriarUnidadeDto("Matriz"));

        Assert.True(resultado.EhSucesso);
        Assert.Equal("UNI000001", resultado.Valor!.Codigo);
        Assert.True(resultado.Valor.Ativo);
        await _gerador.Received().GerarAsync(TipoCodigo.Unidade, Arg.Any<CancellationToken>());
        await _unidadeRepo.Received().AdicionarAsync(Arg.Any<Unidade>(), Arg.Any<CancellationToken>());
        await _uow.Received().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Atualizar_ComIdInexistente_DevolveNaoEncontrado()
    {
        var inexistente = Guid.CreateVersion7();
        _unidadeRepo.ObterPorIdAsync(inexistente, Arg.Any<CancellationToken>()).Returns((Unidade?)null);

        var resultado = await CriarService().AtualizarAsync(inexistente, new AtualizarUnidadeDto("Matriz", true));

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Tipo);
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A inativação é o gatilho da regra central: depois dela, a unidade passa a recusar
    /// novos colaboradores sem que nenhum outro código precise mudar.
    /// </summary>
    [Fact]
    public async Task Atualizar_ComAtivoFalso_BloqueiaNovosColaboradores()
    {
        var unidade = Unidade.Criar("UNI000001", "Matriz");
        _unidadeRepo.ObterPorIdAsync(unidade.Id, Arg.Any<CancellationToken>()).Returns(unidade);

        var resultado = await CriarService().AtualizarAsync(unidade.Id, new AtualizarUnidadeDto("Matriz", false));

        Assert.True(resultado.EhSucesso);
        Assert.False(unidade.PodeReceberColaborador);
    }

    [Fact]
    public async Task AtualizarParcial_ApenasComStatus_PreservaONome()
    {
        var unidade = Unidade.Criar("UNI000001", "Matriz");
        _unidadeRepo.ObterPorIdAsync(unidade.Id, Arg.Any<CancellationToken>()).Returns(unidade);

        await CriarService().AtualizarParcialAsync(unidade.Id, new AtualizarParcialUnidadeDto(null, false));

        Assert.Equal("Matriz", unidade.Nome);
        Assert.False(unidade.Ativo);
    }

    [Fact]
    public async Task AtualizarParcial_ApenasComNome_PreservaOStatus()
    {
        var unidade = Unidade.Criar("UNI000001", "Matriz");
        unidade.Inativar();
        _unidadeRepo.ObterPorIdAsync(unidade.Id, Arg.Any<CancellationToken>()).Returns(unidade);

        await CriarService().AtualizarParcialAsync(unidade.Id, new AtualizarParcialUnidadeDto("Matriz Nova", null));

        Assert.Equal("Matriz Nova", unidade.Nome);
        Assert.False(unidade.Ativo);
    }

    /// <summary>
    /// Cobre o mapeamento da unidade. O preenchimento da coleção de colaboradores depende do
    /// Include do EF e não pode ser simulado aqui — a coleção é privada e só o ORM a povoa.
    /// Esse lado fica coberto pelo teste de integração, contra um banco real.
    /// </summary>
    [Fact]
    public async Task Listar_MapeiaCadaUnidadeParaODtoDeResposta()
    {
        var matriz = Unidade.Criar("UNI000001", "Matriz");
        var filial = Unidade.Criar("UNI000002", "Filial Centro");
        filial.Inativar();

        _unidadeRepo.ListarComColaboradoresAsync(Arg.Any<CancellationToken>()).Returns([matriz, filial]);

        var resultado = await CriarService().ListarAsync();

        Assert.Equal(2, resultado.Valor!.Count);
        Assert.True(resultado.Valor[0].Ativo);
        Assert.False(resultado.Valor[1].Ativo);
        Assert.All(resultado.Valor, dto => Assert.NotNull(dto.Colaboradores));
    }
}
