using GestaoColaboradores.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Api.Controllers;

/// <summary>
/// Controller base (herança na camada de apresentação):
/// traduz o Result Pattern da Application em respostas HTTP + ProblemDetails.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult DeResultado<T>(Result<T> resultado)
    {
        if (resultado.EhSucesso) return Ok(resultado.Valor);
        return Problema(resultado);
    }

    protected ActionResult DeResultado(Result resultado)
    {
        if (resultado.EhSucesso) return NoContent();
        return Problema(resultado);
    }

    /// <summary>POST bem-sucedido: 201 + Location.</summary>
    protected ActionResult Criado<T>(Result<T> resultado, string rota, object valoresRota)
    {
        if (resultado.EhSucesso) return CreatedAtAction(rota, valoresRota, resultado.Valor);
        return Problema(resultado);
    }

    private ObjectResult Problema(Result resultado)
    {
        var status = resultado.Tipo switch
        {
            TipoErro.NaoEncontrado => StatusCodes.Status404NotFound,
            TipoErro.Conflito => StatusCodes.Status409Conflict,
            TipoErro.RegraNegocio => StatusCodes.Status422UnprocessableEntity,
            TipoErro.NaoAutorizado => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };

        return Problem(statusCode: status, detail: resultado.Erro);
    }
}
