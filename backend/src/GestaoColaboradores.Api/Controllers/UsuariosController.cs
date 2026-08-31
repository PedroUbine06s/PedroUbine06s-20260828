using GestaoColaboradores.Application.Dtos;
using GestaoColaboradores.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Api.Controllers;

[Authorize]
public class UsuariosController(IUsuarioService service) : ApiControllerBase
{
    /// <summary>Lista os usuários. Informe <c>ativo</c> para filtrar por status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UsuarioRespostaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult> Listar([FromQuery] bool? ativo, CancellationToken ct) =>
        DeResultado(await service.ListarAsync(ativo, ct));

    /// <summary>Retorna um usuário pelo id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ObterPorId(Guid id, CancellationToken ct) =>
        DeResultado(await service.ObterPorIdAsync(id, ct));

    /// <summary>Cadastra um usuário. O código é gerado pelo sistema; o login deve ser único.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioRespostaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Criar(CriarUsuarioDto dto, CancellationToken ct)
    {
        var resultado = await service.CriarAsync(dto, ct);

        return Criado(resultado, nameof(ObterPorId), u => new { id = u.Id });
    }

    /// <summary>Substitui os campos mutáveis. Senha nula mantém a senha atual.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Atualizar(Guid id, AtualizarUsuarioDto dto, CancellationToken ct) =>
        DeResultado(await service.AtualizarAsync(id, dto, ct));

    /// <summary>Atualização parcial: envie só o que muda, como apenas o status.</summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(UsuarioRespostaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AtualizarParcial(Guid id, AtualizarParcialUsuarioDto dto, CancellationToken ct) =>
        DeResultado(await service.AtualizarParcialAsync(id, dto, ct));
}
