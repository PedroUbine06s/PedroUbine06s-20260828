import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Usa `sessaoValida()` em vez de `estaLogado()` para que um token expirado seja barrado
 * aqui: antes, o guard só via se havia token, a tela protegida montava, disparava a
 * requisição e o 401 é que devolvia a pessoa ao login — um piscar da tela antes do chute.
 */
export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.sessaoValida() ? true : router.createUrlTree(['/login']);
};
