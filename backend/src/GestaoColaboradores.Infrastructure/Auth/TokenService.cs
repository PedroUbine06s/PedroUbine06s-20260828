using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GestaoColaboradores.Application.Interfaces;
using GestaoColaboradores.Domain.Entidades;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GestaoColaboradores.Infrastructure.Auth;

public class TokenService(IOptions<JwtSettings> options) : ITokenService
{
    private readonly JwtSettings _settings = options.Value;

    public TokenGerado GerarToken(Usuario usuario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_settings.ExpiracaoMinutos);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Login),
            new Claim("codigo", usuario.Codigo)
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Emissor,
            audience: _settings.Emissor,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credenciais);

        return new TokenGerado(new JwtSecurityTokenHandler().WriteToken(token), expiraEm);
    }
}
