using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Api.Controllers;

/// <summary>REFERÊNCIA COMPLETA — replique o padrão em Usuarios e Unidades.</summary>
[Authorize]
public class ColaboradoresController(IColaboradorService service) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(List<ColaboradorRespostaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Listar(CancellationToken ct) =>
        DeResultado(await service.ListarAsync(ct));

    [HttpPost]
    [ProducesResponseType(typeof(ColaboradorRespostaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Criar(CriarColaboradorDto dto, CancellationToken ct)
    {
        var resultado = await service.CriarAsync(dto, ct);
        return Criado(resultado, nameof(Listar), new { });
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ColaboradorRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Atualizar(int id, AtualizarColaboradorDto dto, CancellationToken ct) =>
        DeResultado(await service.AtualizarAsync(id, dto, ct));

    /// <summary>PATCH: renomear ou transferir de unidade, sem precisar mandar os dois campos.</summary>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ColaboradorRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> AtualizarParcial(int id, AtualizarParcialColaboradorDto dto, CancellationToken ct) =>
        DeResultado(await service.AtualizarParcialAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Remover(int id, CancellationToken ct) =>
        DeResultado(await service.RemoverAsync(id, ct));
}
