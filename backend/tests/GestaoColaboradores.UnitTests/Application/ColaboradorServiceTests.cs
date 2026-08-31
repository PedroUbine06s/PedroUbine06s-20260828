using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Application.Services;
using GestaoColaboradores.Domain.Entidades;
using NSubstitute;
using Xunit;

namespace GestaoColaboradores.UnitTests.Application;

/// <summary>
/// TESTE DE REFERÊNCIA — cobre a regra central do enunciado no nível do service.
/// Padrão AAA (Arrange / Act / Assert) com dependências mockadas via NSubstitute.
/// </summary>
public class ColaboradorServiceTests
{
    private readonly IColaboradorRepository _colaboradorRepo = Substitute.For<IColaboradorRepository>();
    private readonly IUnidadeRepository _unidadeRepo = Substitute.For<IUnidadeRepository>();
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private ColaboradorService CriarService() =>
        new(_colaboradorRepo, _unidadeRepo, _usuarioRepo, _uow);

    [Fact]
    public async Task CriarAsync_ComUnidadeInativa_DeveRetornarFalhaDeRegraDeNegocio()
    {
        // Arrange — NOTA: Unidade.Criar precisa estar implementada para este teste compilar/passar
        var unidade = Unidade.Criar("UNI-001", "Filial Centro");
        unidade.Inativar();
        _colaboradorRepo.ExisteCodigoAsync("COL-001", Arg.Any<CancellationToken>()).Returns(false);
        _unidadeRepo.ObterPorCodigoAsync("UNI-001", Arg.Any<CancellationToken>()).Returns(unidade);

        var dto = new CriarColaboradorDto("COL-001", "Maria Silva", "UNI-001", "USR-001");

        // Act
        var resultado = await CriarService().CriarAsync(dto);

        // Assert
        Assert.False(resultado.EhSucesso);
        Assert.Equal(TipoErro.RegraNegocio, resultado.Tipo);
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>()); // nada persistido
    }

    [Fact]
    public async Task CriarAsync_ComCodigoDuplicado_DeveRetornarConflito()
    {
        // TODO: ExisteCodigoAsync → true; asserta TipoErro.Conflito
        await Task.CompletedTask;
    }

    // TODO:
    // - CriarAsync_ComUnidadeInexistente_DeveRetornarNaoEncontrado
    // - CriarAsync_ComDadosValidos_DevePersistirECommitar (verificar AdicionarAsync + CommitAsync recebidos)
}
