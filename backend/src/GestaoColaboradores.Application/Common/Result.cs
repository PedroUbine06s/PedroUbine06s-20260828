namespace GestaoColaboradores.Application.Common;

public enum TipoErro
{
    NaoEncontrado,   // → 404
    Conflito,        // → 409 (código/login duplicado)
    RegraNegocio,    // → 422 (ex.: unidade inativa)
    Validacao,       // → 400
    NaoAutorizado    // → 401
}

/// <summary>
/// Result Pattern: falha de regra de negócio não é exceção.
/// O controller base traduz TipoErro em status HTTP.
/// </summary>
public class Result
{
    public bool EhSucesso { get; }
    public string? Erro { get; }
    public TipoErro? Tipo { get; }

    protected Result(bool sucesso, string? erro, TipoErro? tipo)
    {
        EhSucesso = sucesso;
        Erro = erro;
        Tipo = tipo;
    }

    public static Result Sucesso() => new(true, null, null);
    public static Result Falha(string erro, TipoErro tipo) => new(false, erro, tipo);
}

public class Result<T> : Result
{
    public T? Valor { get; }

    private Result(bool sucesso, T? valor, string? erro, TipoErro? tipo)
        : base(sucesso, erro, tipo) => Valor = valor;

    public static Result<T> Sucesso(T valor) => new(true, valor, null, null);
    public static new Result<T> Falha(string erro, TipoErro tipo) => new(false, default, erro, tipo);
}
