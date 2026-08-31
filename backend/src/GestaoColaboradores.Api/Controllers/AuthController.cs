using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Api.Controllers;

public class AuthController(IAuthService authService) : ApiControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Login(LoginDto dto, CancellationToken ct) =>
        DeResultado(await authService.LoginAsync(dto, ct));
}
