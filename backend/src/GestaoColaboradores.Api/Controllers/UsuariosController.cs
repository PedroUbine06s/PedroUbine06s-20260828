using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Api.Controllers;

[Authorize]
public class UsuariosController(IUsuarioService service) : ApiControllerBase
{
    /// <summary>GET /api/v1/usuarios?ativo=true — filtro por status é requisito do enunciado.</summary>
    [HttpGet]
    public async Task<ActionResult> Listar([FromQuery] bool? ativo, CancellationToken ct) =>
        DeResultado(await service.ListarAsync(ativo, ct));

    [HttpPost]
    public async Task<ActionResult> Criar(CriarUsuarioDto dto, CancellationToken ct)
    {
        // TODO: seguir o padrão de ColaboradoresController (201 + Location)
        throw new NotImplementedException();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Atualizar(int id, AtualizarUsuarioDto dto, CancellationToken ct)
    {
        // TODO — lembre: o DTO já restringe a senha e status por contrato
        throw new NotImplementedException();
    }
}
