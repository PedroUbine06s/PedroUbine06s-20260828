using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Api.Controllers;

[Authorize]
public class UnidadesController(IUnidadeService service) : ApiControllerBase
{
    /// <summary>Requisito: listar todas as unidades e seus colaboradores relacionados.</summary>
    [HttpGet]
    public async Task<ActionResult> Listar(CancellationToken ct) =>
        DeResultado(await service.ListarAsync(ct));

    [HttpPost]
    public async Task<ActionResult> Criar(CriarUnidadeDto dto, CancellationToken ct)
    {
        var resultado = await service.CriarAsync(dto, ct);
        return Criado(resultado, nameof(Listar), new { });
    }

    /// <summary>Inativar por aqui: a partir daí o POST de colaborador nessa unidade responde 422.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult> Atualizar(int id, AtualizarUnidadeDto dto, CancellationToken ct) =>
        DeResultado(await service.AtualizarAsync(id, dto, ct));

    /// <summary>PATCH: inativar mandando apenas {"ativo": false}, sem reenviar o nome.</summary>
    [HttpPatch("{id:int}")]
    public async Task<ActionResult> AtualizarParcial(int id, AtualizarParcialUnidadeDto dto, CancellationToken ct) =>
        DeResultado(await service.AtualizarParcialAsync(id, dto, ct));
}
