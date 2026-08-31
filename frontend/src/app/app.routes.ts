import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'colaboradores' },
      { path: 'usuarios', loadComponent: () => import('./features/usuarios/usuarios-lista.component').then(m => m.UsuariosListaComponent) },
      { path: 'colaboradores', loadComponent: () => import('./features/colaboradores/colaboradores-lista.component').then(m => m.ColaboradoresListaComponent) },
      { path: 'unidades', loadComponent: () => import('./features/unidades/unidades-lista.component').then(m => m.UnidadesListaComponent) }
      // TODO: rotas de formulário (novo/editar) por feature
    ]
  },
  { path: '**', redirectTo: '' }
];
