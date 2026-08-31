using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Application.Services;
using GestaoColaboradores.Domain.Entidades;
using NSubstitute;
using Xunit;

namespace GestaoColaboradores.UnitTests.Application;

/// <summary>
/// As decisões de segurança do login, cobradas por teste: mensagem indistinguível entre
/// login inexistente e senha errada, e senha conferida antes do status.
/// </summary>
public class AuthServiceTests
{
    private readonly IUsuarioRepository _usuarioRepo = Substitute.For<IUsuarioRepository>();
    private readonly IPasswordHasher _hasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();

    private AuthService CriarService() => new(_usuarioRepo, _hasher, _tokenService);

    private static Usuario UsuarioAtivo() => Usuario.Criar("USR-001", "admin", "hash-armazenado");

    private static Usuario UsuarioInativo()
    {
        var usuario = UsuarioAtivo();
        usuario.Inativar();
        return usuario;
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_DevolveToken()
    {
        var expiraEm = DateTime.UtcNow.AddHours(1);
        _usuarioRepo.ObterPorLoginAsync("admin", Arg.Any<CancellationToken>()).Returns(UsuarioAtivo());
        _hasher.Verificar("senha-certa", "hash-armazenado").Returns(true);
        _tokenService.GerarToken(Arg.Any<Usuario>()).Returns(new TokenGerado("token-jwt", expiraEm));

        var resultado = await CriarService().LoginAsync(new LoginDto("admin", "senha-certa"));

        Assert.True(resultado.EhSucesso);
        Assert.Equal("token-jwt", resultado.Valor!.Token);
        Assert.Equal(expiraEm, resultado.Valor.ExpiraEm);
    }

    /// <summary>
    /// Se as mensagens diferissem, um atacante descobriria quais logins existem apenas
    /// comparando as respostas — enumeração de usuários.
    /// </summary>
    [Fact]
    public async Task Login_NaoDistingueLoginInexistenteDeSenhaErrada()
    {
        _usuarioRepo.ObterPorLoginAsync("fantasma", Arg.Any<CancellationToken>()).Returns((Usuario?)null);
        var resultadoLoginInexistente = await CriarService().LoginAsync(new LoginDto("fantasma", "qualquer"));

        _usuarioRepo.ObterPorLoginAsync("admin", Arg.Any<CancellationToken>()).Returns(UsuarioAtivo());
        _hasher.Verificar("senha-errada", "hash-armazenado").Returns(false);
        var resultadoSenhaErrada = await CriarService().LoginAsync(new LoginDto("admin", "senha-errada"));

        Assert.False(resultadoLoginInexistente.EhSucesso);
        Assert.False(resultadoSenhaErrada.EhSucesso);
        Assert.Equal(resultadoLoginInexistente.Erro, resultadoSenhaErrada.Erro);
        Assert.Equal(resultadoLoginInexistente.Tipo, resultadoSenhaErrada.Tipo);
    }

    [Fact]
    public async Task Login_ComUsuarioInativo_Recusa()
    {
        _usuarioRepo.ObterPorLoginAsync("admin", Arg.Any<CancellationToken>()).Returns(UsuarioInativo());
        _hasher.Verificar("senha-certa", "hash-armazenado").Returns(true);

        var resultado = await CriarService().LoginAsync(new LoginDto("admin", "senha-certa"));

        Assert.False(resultado.EhSucesso);
        Assert.Equal(TipoErro.NaoAutorizado, resultado.Tipo);
    }

    /// <summary>
    /// A senha é conferida antes do status: só quem já provou ter a credencial correta
    /// descobre que a conta está desativada.
    /// </summary>
    [Fact]
    public async Task Login_ComUsuarioInativoESenhaErrada_NaoRevelaQueAContaExiste()
    {
        _usuarioRepo.ObterPorLoginAsync("admin", Arg.Any<CancellationToken>()).Returns(UsuarioInativo());
        _hasher.Verificar("senha-errada", "hash-armazenado").Returns(false);

        var resultado = await CriarService().LoginAsync(new LoginDto("admin", "senha-errada"));

        Assert.Contains("inválidos", resultado.Erro!, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("inativo", resultado.Erro!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_QuandoFalha_NaoGeraToken()
    {
        _usuarioRepo.ObterPorLoginAsync("fantasma", Arg.Any<CancellationToken>()).Returns((Usuario?)null);

        await CriarService().LoginAsync(new LoginDto("fantasma", "qualquer"));

        _tokenService.DidNotReceive().GerarToken(Arg.Any<Usuario>());
    }
}
