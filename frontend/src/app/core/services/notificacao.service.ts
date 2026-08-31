import { Injectable, signal } from '@angular/core';

export type TipoNotificacao = 'error' | 'success' | 'info' | 'warning';

export interface Notificacao {
  id: number;
  tipo: TipoNotificacao;
  titulo: string;
  mensagem: string;
}

/** Tempo até o toast sumir sozinho. Erros ficam mais tempo por serem acionáveis. */
const DURACAO_MS: Record<TipoNotificacao, number> = {
  error: 8000,
  warning: 6000,
  success: 4000,
  info: 4000
};

/**
 * Fila de notificações exibida pelo NotificacoesComponent.
 *
 * O estado vive em signal e o componente apenas o lê: o serviço não conhece
 * a árvore de componentes, então qualquer camada — inclusive um interceptor
 * HTTP, que não é um componente — pode notificar.
 */
@Injectable({ providedIn: 'root' })
export class NotificacaoService {
  private readonly fila = signal<Notificacao[]>([]);
  private proximoId = 0;

  readonly notificacoes = this.fila.asReadonly();

  erro(mensagem: string, titulo = 'Erro'): void {
    this.adicionar('error', titulo, mensagem);
  }

  sucesso(mensagem: string, titulo = 'Pronto'): void {
    this.adicionar('success', titulo, mensagem);
  }

  aviso(mensagem: string, titulo = 'Atenção'): void {
    this.adicionar('warning', titulo, mensagem);
  }

  remover(id: number): void {
    this.fila.update(atual => atual.filter(n => n.id !== id));
  }

  private adicionar(tipo: TipoNotificacao, titulo: string, mensagem: string): void {
    const id = this.proximoId++;
    this.fila.update(atual => [...atual, { id, tipo, titulo, mensagem }]);

    setTimeout(() => this.remover(id), DURACAO_MS[tipo]);
  }
}
