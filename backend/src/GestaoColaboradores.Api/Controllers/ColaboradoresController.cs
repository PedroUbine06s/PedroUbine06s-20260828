using GestaoColaboradores.Application.Common;
using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Api.Controllers;

[Authorize]
public class ColaboradoresController(IColaboradorService service) : ApiControllerBase
{
    /// <summary>Lista os colaboradores com a unidade de cada um.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PaginaDto<ColaboradorRespostaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Listar([FromQuery] PaginacaoQuery paginacao, CancellationToken ct) =>
        DeResultado(await service.ListarAsync(paginacao, ct));

    /// <summary>Retorna um colaborador pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ColaboradorRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ObterPorId(Guid id, CancellationToken ct) =>
        DeResultado(await service.ObterPorIdAsync(id, ct));

    /// <summary>
    /// Cadastra um colaborador vinculado a uma unidade e a um usuário.
    /// Unidade inativa recusa o cadastro com 422.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ColaboradorRespostaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Criar(CriarColaboradorDto dto, CancellationToken ct)
    {
        var resultado = await service.CriarAsync(dto, ct);

        return Criado(resultado, nameof(ObterPorId), c => new { id = c.Id });
    }

    /// <summary>Substitui nome e unidade. Exige os dois campos.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ColaboradorRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> Atualizar(Guid id, AtualizarColaboradorDto dto, CancellationToken ct) =>
        DeResultado(await service.AtualizarAsync(id, dto, ct));

    /// <summary>Atualização parcial: renomeia ou transfere, sem exigir os dois campos.</summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ColaboradorRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> AtualizarParcial(Guid id, AtualizarParcialColaboradorDto dto, CancellationToken ct) =>
        DeResultado(await service.AtualizarParcialAsync(id, dto, ct));

    /// <summary>Remove o colaborador e inativa o usuário vinculado, na mesma transação.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Remover(Guid id, CancellationToken ct) =>
        DeResultado(await service.RemoverAsync(id, ct));
}
