import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

export interface TokenResposta {
  token: string;
  expiraEm: string;
}

const CHAVE_TOKEN = 'gestao.token';

/**
 * Lê o `exp` do payload do JWT, em milissegundos. Devolve null quando o token não é
 * decodificável — um valor guardado corrompido não deve derrubar a aplicação no boot.
 *
 * Isto é checagem de experiência, não de segurança: o payload é apenas base64, qualquer
 * um forja um `exp` no futuro. Quem valida a assinatura é a API, e é ela que continua
 * decidindo o 401.
 */
function expiracaoEmMs(token: string): number | null {
  try {
    const payload = token.split('.')[1];
    if (!payload) return null;

    const json = JSON.parse(atob(payload.replace(/-/g, '+').replace(/_/g, '/')));

    return typeof json.exp === 'number' ? json.exp * 1000 : null;
  } catch {
    return null;
  }
}

function expirado(token: string): boolean {
  const em = expiracaoEmMs(token);

  // Token sem exp legível segue em uso: a API dirá se presta.
  return em !== null && em <= Date.now();
}

/** Descarta já no boot um token guardado que não vale mais. */
function tokenGuardado(): string | null {
  const token = localStorage.getItem(CHAVE_TOKEN);
  if (token === null) return null;

  if (expirado(token)) {
    localStorage.removeItem(CHAVE_TOKEN);
    return null;
  }

  return token;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly token = signal<string | null>(tokenGuardado());

  /** Para o template: reflete a sessão em memória. */
  readonly estaLogado = computed(() => this.token() !== null);

  /**
   * Para o guard: reavalia a expiração a cada navegação.
   *
   * `estaLogado` é um computed e só recalcula quando o signal muda — o tempo passar não
   * invalida cache. Uma sessão que expira com a aba aberta continuaria "logada" até a
   * próxima requisição. Aqui a conta é refeita na hora, e encerrar a sessão atualiza o
   * signal, que por sua vez esconde o cabeçalho.
   */
  sessaoValida(): boolean {
    const token = this.token();
    if (token === null) return false;

    if (expirado(token)) {
      this.logout();
      return false;
    }

    return true;
  }

  obterToken(): string | null {
    return this.token();
  }

  login(login: string, senha: string): Observable<TokenResposta> {
    return this.http.post<TokenResposta>('/api/v1/auth/login', { login, senha }).pipe(
      tap(resposta => {
        localStorage.setItem(CHAVE_TOKEN, resposta.token);
        this.token.set(resposta.token);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(CHAVE_TOKEN);
    this.token.set(null);
  }
}
