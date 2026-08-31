import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Tratamento central de erros HTTP:
 * - 401 → limpa sessão e volta ao login
 * - demais → extrai o "detail" do ProblemDetails vindo da API
 */
export const erroInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((erro: HttpErrorResponse) => {
      if (erro.status === 401 && !req.url.includes('/auth/login')) {
        auth.logout();
        router.navigate(['/login']);
      }

      // TODO: substituir por um serviço de toast/notificação
      const mensagem = erro.error?.detail ?? 'Erro inesperado. Tente novamente.';
      console.error(mensagem);

      return throwError(() => erro);
    })
  );
};
