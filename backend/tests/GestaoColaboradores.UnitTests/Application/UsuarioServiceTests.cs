using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Application.Services;
using GestaoColaboradores.Domain.Entidades;
using NSubstitute;
using Xunit;

namespace GestaoColaboradores.UnitTests.Application;

public class UsuarioServiceTests
{
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private UsuarioService CriarService() => new(_usuarioRepo, _hasher, _uow);

    public UsuarioServiceTests()
    {
        _hasher.Hash(Arg.Any<string>()).Returns("hash-gerado");
    }

    [Fact]
    public async Task Criar_ComCodigoDuplicado_DevolveConflito()
    {
        _usuarioRepo.ExisteCodigoAsync("USR-001", Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await CriarService().CriarAsync(new CriarUsuarioDto("USR-001", "admin", "senha123", true));

        Assert.Equal(TipoErro.Conflito, resultado.Tipo);
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Criar_ComLoginDuplicado_DevolveConflito()
    {
        _usuarioRepo.ExisteCodigoAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        _usuarioRepo.ExisteLoginAsync("admin", Arg.Any<CancellationToken>()).Returns(true);

        var resultado = await CriarService().CriarAsync(new CriarUsuarioDto("USR-002", "admin", "senha123", true));

        Assert.Equal(TipoErro.Conflito, resultado.Tipo);
        await _uow.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>A senha em texto puro nunca pode chegar à entidade nem à resposta.</summary>
    [Fact]
    public async Task Criar_ArmazenaOHashENaoASenhaDigitada()
    {
        Usuario? persistido = null;
        await _usuarioRepo.AdicionarAsync(Arg.Do<Usuario>(u => persistido = u), Arg.Any<CancellationToken>());

        var resultado = await CriarService().CriarAsync(new CriarUsuarioDto("USR-002", "maria", "senhaSecreta", true));

        Assert.True(resultado.EhSucesso);
        _hasher.Received().Hash("senhaSecreta");
        Assert.Equal("hash-gerado", persistido!.SenhaHash);
        Assert.DoesNotContain("senhaSecreta", System.Text.Json.JsonSerializer.Serialize(resultado.Valor));
    }

    [Fact]
    public async Task Criar_ComAtivoFalso_NasceInativo()
    {
        Usuario? persistido = null;
        await _usuarioRepo.AdicionarAsync(Arg.Do<Usuario>(u => persistido = u), Arg.Any<CancellationToken>());

        await CriarService().CriarAsync(new CriarUsuarioDto("USR-002", "maria", "senha123", false));

        Assert.False(persistido!.Ativo);
    }

    [Fact]
    public async Task Atualizar_ComIdInexistente_DevolveNaoEncontrado()
    {
        _usuarioRepo.ObterPorIdAsync(99, Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        var resultado = await CriarService().AtualizarAsync(99, new AtualizarUsuarioDto(null, true));

        Assert.Equal(TipoErro.NaoEncontrado, resultado.Tipo);
    }

    /// <summary>Senha nula significa "não alterar" — o hasher nem deve ser chamado.</summary>
    [Fact]
    public async Task Atualizar_SemSenha_MantemAAtual()
    {
        var usuario = Usuario.Criar("USR-001", "admin", "hash-original");
        _usuarioRepo.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(usuario);

        await CriarService().AtualizarAsync(1, new AtualizarUsuarioDto(null, false));

        _hasher.DidNotReceive().Hash(Arg.Any<string>());
        Assert.Equal("hash-original", usuario.SenhaHash);
        Assert.False(usuario.Ativo);
    }

    [Fact]
    public async Task Atualizar_ComSenha_AplicaOHashNovo()
    {
        var usuario = Usuario.Criar("USR-001", "admin", "hash-original");
        _usuarioRepo.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(usuario);

        await CriarService().AtualizarAsync(1, new AtualizarUsuarioDto("novaSenha123", true));

        _hasher.Received().Hash("novaSenha123");
        Assert.Equal("hash-gerado", usuario.SenhaHash);
    }

    [Fact]
    public async Task AtualizarParcial_ApenasComStatus_NaoTocaNaSenha()
    {
        var usuario = Usuario.Criar("USR-001", "admin", "hash-original");
        _usuarioRepo.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(usuario);

        await CriarService().AtualizarParcialAsync(1, new AtualizarParcialUsuarioDto(null, false));

        _hasher.DidNotReceive().Hash(Arg.Any<string>());
        Assert.Equal("hash-original", usuario.SenhaHash);
        Assert.False(usuario.Ativo);
    }

    [Fact]
    public async Task AtualizarParcial_ApenasComSenha_NaoTocaNoStatus()
    {
        var usuario = Usuario.Criar("USR-001", "admin", "hash-original");
        usuario.Inativar();
        _usuarioRepo.ObterPorIdAsync(1, Arg.Any<CancellationToken>()).Returns(usuario);

        await CriarService().AtualizarParcialAsync(1, new AtualizarParcialUsuarioDto("novaSenha123", null));

        Assert.Equal("hash-gerado", usuario.SenhaHash);
        Assert.False(usuario.Ativo); // permanece como estava
    }

    [Fact]
    public async Task Listar_SemFiltro_TrazTodos()
    {
        _usuarioRepo.ListarAsync(Arg.Any<CancellationToken>()).Returns([Usuario.Criar("USR-001", "admin", "h")]);

        var resultado = await CriarService().ListarAsync(null);

        Assert.Single(resultado.Valor!);
        await _usuarioRepo.Received().ListarAsync(Arg.Any<CancellationToken>());
        await _usuarioRepo.DidNotReceive().ListarPorStatusAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Listar_ComFiltro_DelegaAoRepositorioFiltrado()
    {
        _usuarioRepo.ListarPorStatusAsync(false, Arg.Any<CancellationToken>()).Returns([]);

        await CriarService().ListarAsync(false);

        await _usuarioRepo.Received().ListarPorStatusAsync(false, Arg.Any<CancellationToken>());
        await _usuarioRepo.DidNotReceive().ListarAsync(Arg.Any<CancellationToken>());
    }
}
