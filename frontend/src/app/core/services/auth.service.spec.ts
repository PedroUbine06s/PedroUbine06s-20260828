import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { AuthService } from './auth.service';

const CHAVE = 'gestao.token';

function base64url(valor: unknown): string {
  return btoa(JSON.stringify(valor)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

/** Monta um JWT com o payload pedido. A assinatura é irrelevante: o front não a confere. */
function tokenCom(payload: Record<string, unknown>): string {
  return `${base64url({ alg: 'HS256', typ: 'JWT' })}.${base64url(payload)}.assinatura`;
}

function emSegundos(deslocamento: number): number {
  return Math.floor(Date.now() / 1000) + deslocamento;
}

/** Instancia o serviço só agora, para que ele leia o localStorage já preparado. */
function criarServico(): AuthService {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting()]
  });

  return TestBed.inject(AuthService);
}

describe('AuthService', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.resetTestingModule();
  });

  // Devolve o relógio real mesmo que uma asserção falhe no meio do teste.
  afterEach(() => vi.useRealTimers());

  describe('sessão guardada', () => {
    it('continua logado com um token ainda válido', () => {
      localStorage.setItem(CHAVE, tokenCom({ exp: emSegundos(3600) }));

      const auth = criarServico();

      expect(auth.estaLogado()).toBe(true);
      expect(auth.sessaoValida()).toBe(true);
    });

    it('descarta no boot um token já vencido', () => {
      localStorage.setItem(CHAVE, tokenCom({ exp: emSegundos(-10) }));

      const auth = criarServico();

      expect(auth.estaLogado()).toBe(false);
      // Não basta ignorar em memória: o valor inútil sai do armazenamento.
      expect(localStorage.getItem(CHAVE)).toBeNull();
    });

    it('começa deslogado quando não há nada guardado', () => {
      expect(criarServico().estaLogado()).toBe(false);
    });
  });

  describe('tokens que não dá para interpretar', () => {
    // Sem exp legível, quem decide é a API: o front não pode inventar uma expiração.
    it('mantém em uso um token sem exp', () => {
      localStorage.setItem(CHAVE, tokenCom({ sub: 'alguem' }));

      expect(criarServico().sessaoValida()).toBe(true);
    });

    it('mantém em uso um token com exp que não é número', () => {
      localStorage.setItem(CHAVE, tokenCom({ exp: 'amanhã' }));

      expect(criarServico().sessaoValida()).toBe(true);
    });

    // Um valor corrompido no armazenamento não pode derrubar a aplicação no boot.
    it.each([
      ['sem pontos', 'nada-disso-e-um-jwt'],
      ['payload inválido', 'a.@@@.c']
    ])('não lança com um token %s', (_caso, valor) => {
      localStorage.setItem(CHAVE, valor);

      expect(() => criarServico().sessaoValida()).not.toThrow();
    });
  });

  describe('sessaoValida', () => {
    it('encerra a sessão quando o token venceu com a aba aberta', () => {
      // Vence daqui a um segundo: válido no boot, expirado na navegação seguinte.
      localStorage.setItem(CHAVE, tokenCom({ exp: emSegundos(1) }));
      const auth = criarServico();
      expect(auth.estaLogado()).toBe(true);

      // 2s depois o mesmo token já não vale, sem que signal algum tenha mudado.
      vi.useFakeTimers();
      vi.setSystemTime(Date.now() + 2000);

      expect(auth.sessaoValida()).toBe(false);
      // Encerrar a sessão precisa refletir no template, não só no retorno.
      expect(auth.estaLogado()).toBe(false);
    });
  });

  describe('login e logout', () => {
    it('guarda o token devolvido pela API', () => {
      const auth = criarServico();
      const http = TestBed.inject(HttpTestingController);
      const token = tokenCom({ exp: emSegundos(3600) });

      auth.login('admin', 'admin123').subscribe();
      const req = http.expectOne('/api/v1/auth/login');
      expect(req.request.body).toEqual({ login: 'admin', senha: 'admin123' });
      req.flush({ token, expiraEm: new Date().toISOString() });

      expect(auth.estaLogado()).toBe(true);
      expect(localStorage.getItem(CHAVE)).toBe(token);
      http.verify();
    });

    it('limpa memória e armazenamento ao sair', () => {
      localStorage.setItem(CHAVE, tokenCom({ exp: emSegundos(3600) }));
      const auth = criarServico();

      auth.logout();

      expect(auth.estaLogado()).toBe(false);
      expect(localStorage.getItem(CHAVE)).toBeNull();
    });
  });
});
