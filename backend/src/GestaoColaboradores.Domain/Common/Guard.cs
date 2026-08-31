using System.Runtime.CompilerServices;

namespace GestaoColaboradores.Domain.Common;

/// <summary>
/// Validação de invariantes compartilhada pelas entidades.
/// Devolve o texto já normalizado, de modo que validar e limpar sejam sempre o mesmo passo —
/// não existe caminho em que o valor seja aceito sem passar por Trim.
/// </summary>
public static class Guard
{
    /// <param name="campo">
    /// Preenchido automaticamente pelo compilador com a expressão passada em
    /// <paramref name="valor"/>, o que dispensa repetir nameof() em cada chamada.
    /// </param>
    public static string TextoObrigatorio(
        string? valor,
        int tamanhoMaximo,
        [CallerArgumentExpression(nameof(valor))] string campo = "")
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException($"O campo '{campo}' é obrigatório.", campo);

        var texto = valor.Trim();

        if (texto.Length > tamanhoMaximo)
            throw new ArgumentException(
                $"O campo '{campo}' deve ter no máximo {tamanhoMaximo} caracteres.", campo);

        return texto;
    }
}
