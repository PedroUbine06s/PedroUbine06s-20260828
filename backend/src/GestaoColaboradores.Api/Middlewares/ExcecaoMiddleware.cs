using GestaoColaboradores.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace GestaoColaboradores.Api.Middlewares;

/// <summary>
/// Última linha de defesa: qualquer exceção não tratada vira ProblemDetails (RFC 7807),
/// nunca um stack trace vazando para o cliente.
///
/// A violação de unicidade recebe tratamento próprio porque é erro do cliente, não do
/// servidor: mandar dois registros com o mesmo valor único merece 409, não 500.
/// </summary>
public class ExcecaoMiddleware(RequestDelegate next, ILogger<ExcecaoMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ConflitoDeConcorrenciaException ex)
        {
            logger.LogWarning(ex, "Conflito de concorrência em {Metodo} {Rota}",
                context.Request.Method, context.Request.Path);

            await EscreverAsync(context, StatusCodes.Status409Conflict,
                "Conflito de concorrência.", ex.Message);
        }
        catch (ConflitoDePersistenciaException ex)
        {
            // A mensagem interna carrega o nome da restrição — útil no log, ruído (e pista
            // sobre o schema) para quem consome a API.
            logger.LogWarning(ex, "Conflito de unicidade em {Metodo} {Rota}",
                context.Request.Method, context.Request.Path);

            await EscreverAsync(context, StatusCodes.Status409Conflict,
                "Conflito de dados.",
                "Já existe um registro com um dos valores informados.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro não tratado em {Metodo} {Rota}", context.Request.Method, context.Request.Path);

            await EscreverAsync(context, StatusCodes.Status500InternalServerError,
                "Erro interno do servidor.",
                "Ocorreu um erro inesperado. Tente novamente mais tarde.");
        }
    }

    private static Task EscreverAsync(HttpContext context, int status, string titulo, string detalhe)
    {
        context.Response.StatusCode = status;

        // O contentType vai no próprio WriteAsJsonAsync: definir Response.ContentType antes
        // não adianta, porque o método o sobrescreve com application/json.
        return context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = detalhe,
                Instance = context.Request.Path
            },
            options: null,
            contentType: "application/problem+json");
    }
}
