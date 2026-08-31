namespace GestaoColaboradores.Application.Common;

/// <summary>
/// Violação de restrição de unicidade detectada pelo banco na hora de gravar.
///
/// Os serviços checam duplicidade antes de inserir, e é de lá que vem o 409 com mensagem
/// específica. Esta exceção cobre o que a checagem prévia não alcança: duas requisições
/// simultâneas que passam pela verificação juntas e só colidem no índice. Sem ela, esse
/// caso viraria 500 — um erro de cliente disfarçado de falha do servidor.
///
/// A tradução acontece na infraestrutura, que é quem conhece o dialeto do banco; a
/// aplicação e a API lidam apenas com este tipo.
/// </summary>
public class ConflitoDePersistenciaException(string mensagem, Exception? innerException = null)
    : Exception(mensagem, innerException);

/// <summary>
/// A linha foi alterada por outra transação entre a leitura e a gravação (detectado pelo
/// token de concorrência). É um 409 como o de unicidade, mas com causa e mensagem próprias:
/// aqui não há valor duplicado, há uma alteração perdida que se recusou a acontecer.
/// </summary>
public class ConflitoDeConcorrenciaException(string mensagem, Exception? innerException = null)
    : Exception(mensagem, innerException);
