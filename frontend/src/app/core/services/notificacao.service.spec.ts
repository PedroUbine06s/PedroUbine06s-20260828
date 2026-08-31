import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { NotificacaoService } from './notificacao.service';

describe('NotificacaoService', () => {
  let servico: NotificacaoService;

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({});
    servico = TestBed.inject(NotificacaoService);
  });

  afterEach(() => vi.useRealTimers());

  it('começa sem notificação alguma', () => {
    expect(servico.notificacoes()).toEqual([]);
  });

  it('enfileira o erro com tipo e mensagem', () => {
    servico.erro('Unidade inativa.');

    const [notificacao] = servico.notificacoes();
    expect(notificacao.tipo).toBe('error');
    expect(notificacao.mensagem).toBe('Unidade inativa.');
  });

  it('mantém as notificações simultâneas em vez de substituir a anterior', () => {
    servico.erro('primeira');
    servico.sucesso('segunda');

    expect(servico.notificacoes().map(n => n.mensagem)).toEqual(['primeira', 'segunda']);
  });

  it('remove sozinha depois da duração do tipo', () => {
    servico.sucesso('salvo');
    expect(servico.notificacoes()).toHaveLength(1);

    vi.advanceTimersByTime(4000);

    expect(servico.notificacoes()).toEqual([]);
  });

  // Erro fica mais tempo que sucesso: some no tempo dele, não no do sucesso.
  it('mantém o erro visível além da duração de um sucesso', () => {
    servico.erro('falhou');

    vi.advanceTimersByTime(4000);
    expect(servico.notificacoes()).toHaveLength(1);

    vi.advanceTimersByTime(4000);
    expect(servico.notificacoes()).toEqual([]);
  });

  it('remove apenas a notificação pedida', () => {
    servico.erro('fica');
    servico.erro('sai');
    const alvo = servico.notificacoes()[1];

    servico.remover(alvo.id);

    expect(servico.notificacoes().map(n => n.mensagem)).toEqual(['fica']);
  });
});
