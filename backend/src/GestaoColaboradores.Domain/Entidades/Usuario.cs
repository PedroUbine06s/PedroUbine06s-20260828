using GestaoColaboradores.Domain.Common;

namespace GestaoColaboradores.Domain.Entidades;

public class Usuario : EntidadeAtivavel
{
    public const int TamanhoMaximoLogin = 100;
    public const int TamanhoMaximoSenhaHash = 200;

    public string Login { get; private set; } = string.Empty;
    public string SenhaHash { get; private set; } = string.Empty;

    private Usuario() { } // EF Core

    public static Usuario Criar(string codigo, string login, string senhaHash)
    {
        return new Usuario
        {
            Codigo = Guard.TextoObrigatorio(codigo, TamanhoMaximoCodigo),
            Login = Guard.TextoObrigatorio(login, TamanhoMaximoLogin),
            SenhaHash = Guard.TextoObrigatorio(senhaHash, TamanhoMaximoSenhaHash)
        };
    }

    /// <summary>Regra do enunciado: só senha e status são atualizáveis.</summary>
    public void AlterarSenha(string novaSenhaHash)
    {
        SenhaHash = Guard.TextoObrigatorio(novaSenhaHash, TamanhoMaximoSenhaHash);
        MarcarAtualizado();
    }
}
