namespace GestaoColaboradores.Application.Common;

/// <summary>
/// Parâmetros de paginação vindos da query string.
///
/// O tamanho é limitado no próprio construtor porque paginação sem teto não é paginação:
/// bastaria pedir <c>?tamanho=1000000</c> para reproduzir o problema que ela evita.
/// </summary>
public record PaginacaoQuery
{
    public const int TamanhoPadrao = 20;
    public const int TamanhoMaximo = 100;

    private readonly int _pagina = 1;
    private readonly int _tamanho = TamanhoPadrao;

    public int Pagina
    {
        get => _pagina;
        init => _pagina = value < 1 ? 1 : value;
    }

    public int Tamanho
    {
        get => _tamanho;
        init => _tamanho = value switch
        {
            < 1 => TamanhoPadrao,
            > TamanhoMaximo => TamanhoMaximo,
            _ => value
        };
    }

    public int QuantidadeAPular => (Pagina - 1) * Tamanho;
}

/// <summary>
/// Envelope de resposta paginada. O total vem junto para que o cliente saiba quantas páginas
/// existem sem precisar percorrer todas.
/// </summary>
public record PaginaDto<T>(IReadOnlyList<T> Itens, int Pagina, int Tamanho, int Total)
{
    public int TotalDePaginas => Total == 0 ? 0 : (int)Math.Ceiling(Total / (double)Tamanho);
}
