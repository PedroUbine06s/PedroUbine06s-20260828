using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Api.Controllers;

[Authorize]
public class UnidadesController(IUnidadeService service) : ApiControllerBase
{
    /// <summary>Lista todas as unidades com os colaboradores de cada uma.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UnidadeComColaboradoresDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Listar(CancellationToken ct) =>
        DeResultado(await service.ListarAsync(ct));

    /// <summary>Retorna uma unidade pelo id, com seus colaboradores.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UnidadeComColaboradoresDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ObterPorId(Guid id, CancellationToken ct) =>
        DeResultado(await service.ObterPorIdAsync(id, ct));

    /// <summary>Cadastra uma unidade. O código é gerado pelo sistema (UNI000001).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UnidadeRespostaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Criar(CriarUnidadeDto dto, CancellationToken ct)
    {
        var resultado = await service.CriarAsync(dto, ct);

        return Criado(resultado, nameof(ObterPorId), u => new { id = u.Id });
    }

    /// <summary>Substitui nome e status. Exige os dois campos.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UnidadeRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Atualizar(Guid id, AtualizarUnidadeDto dto, CancellationToken ct) =>
        DeResultado(await service.AtualizarAsync(id, dto, ct));

    /// <summary>
    /// Atualização parcial. É por aqui que se inativa uma unidade, enviando apenas
    /// <c>{ "ativo": false }</c> — a partir daí ela recusa novos colaboradores com 422.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(UnidadeRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AtualizarParcial(Guid id, AtualizarParcialUnidadeDto dto, CancellationToken ct) =>
        DeResultado(await service.AtualizarParcialAsync(id, dto, ct));
}
