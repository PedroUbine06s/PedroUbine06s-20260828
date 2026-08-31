import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/** Anexa o Bearer token a toda requisição autenticada. */
export const tokenInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).obterToken();

  if (token) {
    req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
  }

  return next(req);
};
