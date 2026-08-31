namespace GestaoColaboradores.Domain.Common;

/// <summary>
/// Raiz da hierarquia de herança: campos comuns às três entidades do sistema
/// (todas possuem código único e auditoria básica).
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Fonte única do limite: usada na validação do domínio e no schema do banco.</summary>
    public const int TamanhoMaximoCodigo = 20;

    /// <summary>
    /// UUID versão 7: aleatório o bastante para não ser adivinhável e, ao mesmo tempo,
    /// ordenado no tempo. Um UUID v4 puro espalharia as inserções por toda a árvore do
    /// índice e o fragmentaria; o v7 preserva a localidade de um id sequencial.
    ///
    /// Gerado aqui, e não pelo banco: a entidade já nasce com identidade, o que dispensa
    /// gravar o principal antes de vincular o dependente.
    /// </summary>
    public Guid Id { get; protected set; } = Guid.CreateVersion7();
    public string Codigo { get; protected set; } = string.Empty;
    public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; protected set; }

    protected void MarcarAtualizado() => AtualizadoEm = DateTime.UtcNow;
}
