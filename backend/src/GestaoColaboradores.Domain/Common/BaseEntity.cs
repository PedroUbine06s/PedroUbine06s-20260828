namespace GestaoColaboradores.Domain.Common;

/// <summary>
/// Raiz da hierarquia de herança: campos comuns às três entidades do sistema
/// (todas possuem código único e auditoria básica).
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Fonte única do limite: usada na validação do domínio e no schema do banco.</summary>
    public const int TamanhoMaximoCodigo = 20;

    public int Id { get; protected set; }
    public string Codigo { get; protected set; } = string.Empty;
    public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;
    public DateTime? AtualizadoEm { get; protected set; }

    protected void MarcarAtualizado() => AtualizadoEm = DateTime.UtcNow;
}
