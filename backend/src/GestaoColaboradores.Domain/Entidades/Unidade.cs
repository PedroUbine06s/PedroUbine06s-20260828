using GestaoColaboradores.Domain.Common;

namespace GestaoColaboradores.Domain.Entidades;

public class Unidade : EntidadeAtivavel
{
    public const int TamanhoMaximoNome = 150;

    public string Nome { get; private set; } = string.Empty;

    private readonly List<Colaborador> _colaboradores = [];
    public IReadOnlyCollection<Colaborador> Colaboradores => _colaboradores.AsReadOnly();

    /// <summary>
    /// Regra central do enunciado, expressa no domínio:
    /// unidade inativa não permite inclusão de novos colaboradores.
    /// </summary>
    public bool PodeReceberColaborador => Ativo;

    private Unidade() { } // EF Core

    public static Unidade Criar(string codigo, string nome)
    {
        return new Unidade
        {
            Codigo = Guard.TextoObrigatorio(codigo, TamanhoMaximoCodigo),
            Nome = Guard.TextoObrigatorio(nome, TamanhoMaximoNome)
        };
    }

    public void AlterarNome(string nome)
    {
        Nome = Guard.TextoObrigatorio(nome, TamanhoMaximoNome);
        MarcarAtualizado();
    }
}
