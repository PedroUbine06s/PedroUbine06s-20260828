// Espelham os DTOs da API — mesma restrição de contrato do backend.
// Os identificadores são UUID (Guid no backend), portanto `string` aqui.

/**
 * Envelope de resposta paginada devolvido por todas as listagens.
 * `totalDePaginas` é calculado pelo backend e vem junto para que a tela
 * saiba quantas páginas existem sem precisar percorrer todas.
 */
export interface Pagina<T> {
  itens: T[];
  pagina: number;
  tamanho: number;
  total: number;
  totalDePaginas: number;
}

/** Parâmetros de paginação. O backend limita `tamanho` a 100. */
export interface ParametrosPaginacao {
  pagina?: number;
  tamanho?: number;
}

// --- Usuários -------------------------------------------------------------
// A senha nunca aparece em resposta — nem o hash.

export interface Usuario {
  id: string;
  codigo: string;
  login: string;
  ativo: boolean;
}

/** O código é gerado pelo sistema (USR000001), por isso não é informado. */
export interface CriarUsuario {
  login: string;
  senha: string;
  ativo: boolean;
}

/** Por contrato, só senha e status são atualizáveis. Senha ausente = não alterar. */
export interface AtualizarUsuario {
  senha?: string;
  ativo: boolean;
}

// --- Unidades -------------------------------------------------------------

export interface Unidade {
  id: string;
  codigo: string;
  nome: string;
  ativo: boolean;
  /** O GET de unidades já traz os colaboradores de cada uma — requisito do enunciado. */
  colaboradores: Colaborador[];
}

/** O código é gerado pelo sistema (UNI000001). */
export interface CriarUnidade {
  nome: string;
}

export interface AtualizarUnidade {
  nome: string;
  ativo: boolean;
}

/** PATCH: campo ausente significa "não alterar". */
export interface AtualizarParcialUnidade {
  nome?: string;
  ativo?: boolean;
}

// --- Colaboradores --------------------------------------------------------

export interface Colaborador {
  id: string;
  codigo: string;
  nome: string;
  unidadeId: string;
  codigoUnidade: string;
  nomeUnidade: string;
}

/**
 * O código é gerado pelo sistema (COL000001). Unidade e usuário são
 * referenciados pelo id, que é o identificador canônico da API.
 */
export interface CriarColaborador {
  nome: string;
  unidadeId: string;
  usuarioId: string;
}

export interface AtualizarColaborador {
  nome: string;
  unidadeId: string;
}
