using GestaoColaboradores.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GestaoColaboradores.Infrastructure.Persistence;

/// <summary>
/// Numera as entidades a partir de sequences do PostgreSQL.
///
/// A escolha da sequence é a parte que importa: <c>nextval</c> é atômico e nunca devolve o
/// mesmo número a dois chamadores, mesmo sob requisições simultâneas. A alternativa ingênua
/// — ler o maior código e somar um — tem corrida entre a leitura e a gravação, e duas
/// criações concorrentes acabariam colidindo no índice único.
/// </summary>
public class GeradorCodigo(AppDbContext context) : IGeradorCodigo
{
    // O SQL é constante por tipo, e não montado por interpolação, por dois motivos: o nome de
    // uma sequence é um identificador e não pode ser parametrizado, e deixá-lo literal remove
    // qualquer dúvida sobre injeção. Não troque por SqlQuery com string interpolada: ali o
    // nome viraria um parâmetro (@p0) e o banco procuraria uma tabela com esse nome.
    private static (string Prefixo, string Sql) Config(TipoCodigo tipo) => tipo switch
    {
        TipoCodigo.Usuario => ("USR", $"SELECT nextval('{AppDbContext.SequenceUsuarios}') AS \"Value\""),
        TipoCodigo.Unidade => ("UNI", $"SELECT nextval('{AppDbContext.SequenceUnidades}') AS \"Value\""),
        TipoCodigo.Colaborador => ("COL", $"SELECT nextval('{AppDbContext.SequenceColaboradores}') AS \"Value\""),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo))
    };

    public async Task<string> GerarAsync(TipoCodigo tipo, CancellationToken ct = default)
    {
        var (prefixo, sql) = Config(tipo);

        var proximo = await context.Database.SqlQueryRaw<long>(sql).FirstAsync(ct);

        return $"{prefixo}{proximo:D6}";
    }
}
