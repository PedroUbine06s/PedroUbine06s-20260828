using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GestaoColaboradores.Api.Controllers;

public class AuthController(IAuthService authService) : ApiControllerBase
{
    /// <summary>
    /// Autentica contra a tabela de usuários e devolve um Bearer token.
    /// Somente usuário ativo consegue entrar. Use <c>admin</c> / <c>admin123</c> para avaliar,
    /// e informe o token retornado no botão Authorize.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(PoliticasDeLimite.Login)]
    [ProducesResponseType(typeof(TokenRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult> Login(LoginDto dto, CancellationToken ct) =>
        DeResultado(await authService.LoginAsync(dto, ct));
}
