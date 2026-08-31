using GestaoColaboradores.Domain.Common;

namespace GestaoColaboradores.Domain.Entidades;

public class Colaborador : BaseEntity
{
    public const int TamanhoMaximoNome = 150;

    public string Nome { get; private set; } = string.Empty;

    public Guid UnidadeId { get; private set; }
    public Unidade Unidade { get; private set; } = null!;

    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    private Colaborador() { } // EF Core

    /// <summary>
    /// Factory Method: único caminho de criação — a entidade nunca existe em estado inválido.
    /// A regra "unidade inativa não recebe colaborador" é verificada aqui, no domínio.
    /// </summary>
    public static Colaborador Criar(
        string codigo,
        string nome,
        Unidade unidade,
        Usuario usuario)
    {
        ArgumentNullException.ThrowIfNull(unidade);
        ArgumentNullException.ThrowIfNull(usuario);

        codigo = Guard.TextoObrigatorio(codigo, TamanhoMaximoCodigo);
        nome = Guard.TextoObrigatorio(nome, TamanhoMaximoNome);

        if (!unidade.PodeReceberColaborador)
            throw new InvalidOperationException(
                "Unidade inativa não permite inclusão de novos colaboradores.");

        return new Colaborador
        {
            Codigo = codigo,
            Nome = nome,
            Unidade = unidade,
            UnidadeId = unidade.Id,
            Usuario = usuario,
            UsuarioId = usuario.Id
        };
    }

    /// <summary>
    /// Altera o nome do colaborador.
    /// </summary>
    public void AlterarNome(string nome)
    {
        Nome = Guard.TextoObrigatorio(nome, TamanhoMaximoNome);

        MarcarAtualizado();
    }

    /// <summary>
    /// Transfere o colaborador para outra unidade.
    /// A unidade precisa estar ativa e apta a receber colaboradores.
    /// </summary>
    public void AlterarUnidade(Unidade novaUnidade)
    {
        ArgumentNullException.ThrowIfNull(novaUnidade);

        if (!novaUnidade.PodeReceberColaborador)
            throw new InvalidOperationException(
                "Unidade inativa não pode receber colaboradores.");

        Unidade = novaUnidade;
        UnidadeId = novaUnidade.Id;

        MarcarAtualizado();
    }
}
