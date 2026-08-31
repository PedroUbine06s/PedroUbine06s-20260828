namespace GestaoColaboradores.Application.Interfaces;

public enum TipoCodigo
{
    Usuario,
    Unidade,
    Colaborador
}

/// <summary>
/// Gera o código de negócio das entidades (USR000001, UNI000001, COL000001).
///
/// O código deixou de ser entrada do cliente: quem numera é o sistema. A implementação usa
/// sequences do PostgreSQL porque a operação precisa ser atômica — duas requisições
/// simultâneas jamais podem receber o mesmo número. Um "maior valor + 1" na aplicação teria
/// corrida entre a leitura e a gravação.
/// </summary>
public interface IGeradorCodigo
{
    Task<string> GerarAsync(TipoCodigo tipo, CancellationToken ct = default);
}
