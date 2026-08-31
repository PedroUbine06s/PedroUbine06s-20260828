namespace GestaoColaboradores.Domain.Common;

/// <summary>
/// Template Method: define o esqueleto de ativação/inativação.
/// Subclasses podem sobrescrever para adicionar comportamento (ex.: Unidade).
/// </summary>
public abstract class EntidadeAtivavel : BaseEntity
{
    public bool Ativo { get; protected set; } = true;

    public virtual void Ativar()
    {
        Ativo = true;
        MarcarAtualizado();
    }

    public virtual void Inativar()
    {
        Ativo = false;
        MarcarAtualizado();
    }
}
