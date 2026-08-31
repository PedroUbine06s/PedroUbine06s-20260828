// Espelham os DTOs de resposta da API — mesma restrição de contrato do backend.

export interface Usuario {
  id: number;
  codigo: string;
  login: string;
  ativo: boolean;
}

export interface Colaborador {
  id: number;
  codigo: string;
  nome: string;
  codigoUnidade: string;
  nomeUnidade: string;
}

export interface Unidade {
  id: number;
  codigo: string;
  nome: string;
  ativo: boolean;
  colaboradores?: Colaborador[];
}
