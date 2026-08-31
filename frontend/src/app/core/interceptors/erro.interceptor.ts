import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { NotificacaoService } from '../services/notificacao.service';
import { AuthService } from '../services/auth.service';

/** Mensagens para os status em que o ProblemDetails da API não é suficiente. */
function mensagemDe(erro: HttpErrorResponse): string {
  if (erro.status === 0) return 'Não foi possível falar com o servidor. Ele está no ar?';
  if (erro.status === 429) return 'Muitas tentativas seguidas. Aguarde um minuto e tente de novo.';

  return erro.error?.detail ?? 'Erro inesperado. Tente novamente.';
}

/**
 * Tratamento central de erros HTTP:
 * - 401 → limpa a sessão e volta ao login
 * - demais → exibe o "detail" do ProblemDetails devolvido pela API
 *
 * O login trata o próprio erro na tela, então é excluído do toast para não
 * mostrar a mesma mensagem duas vezes.
 */
export const erroInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const notificacao = inject(NotificacaoService);

  const ehLogin = req.url.includes('/auth/login');

  return next(req).pipe(
    catchError((erro: HttpErrorResponse) => {
      if (erro.status === 401 && !ehLogin) {
        auth.logout();
        router.navigate(['/login']);
        notificacao.aviso('Sua sessão expirou. Entre novamente.', 'Sessão encerrada');

        return throwError(() => erro);
      }

      if (!ehLogin) notificacao.erro(mensagemDe(erro));

      return throwError(() => erro);
    })
  );
};
