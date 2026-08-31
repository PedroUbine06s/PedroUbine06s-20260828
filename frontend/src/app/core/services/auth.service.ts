import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

export interface TokenResposta {
  token: string;
  expiraEm: string;
}

const CHAVE_TOKEN = 'gestao.token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly token = signal<string | null>(localStorage.getItem(CHAVE_TOKEN));

  readonly estaLogado = computed(() => this.token() !== null);

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
