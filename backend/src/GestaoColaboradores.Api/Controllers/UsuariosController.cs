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
        var resultado = await service.CriarAsync(dto, ct);
        return Criado(resultado, nameof(Listar), new { });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Atualizar(int id, AtualizarUsuarioDto dto, CancellationToken ct)
    {
        return DeResultado(await service.AtualizarAsync(id, dto, ct));
    }

    /// <summary>PATCH: envie só o que muda — trocar a senha sem mexer no status, por exemplo.</summary>
    [HttpPatch("{id:int}")]
    public async Task<ActionResult> AtualizarParcial(int id, AtualizarParcialUsuarioDto dto, CancellationToken ct) =>
        DeResultado(await service.AtualizarParcialAsync(id, dto, ct));
}
