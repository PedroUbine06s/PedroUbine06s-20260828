using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Api.Middlewares;

/// <summary>
/// Última linha de defesa: qualquer exceção não tratada vira ProblemDetails (RFC 7807),
/// nunca um stack trace vazando para o cliente.
/// </summary>
public class ExcecaoMiddleware(RequestDelegate next, ILogger<ExcecaoMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado em {Metodo} {Rota}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro interno do servidor.",
                Detail = "Ocorreu um erro inesperado. Tente novamente mais tarde.",
                Instance = context.Request.Path
            });
        }
    }
}
