import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { NotificacoesComponent } from './shared/notificacoes.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NotificacoesComponent],
  template: `
    @if (auth.estaLogado()) {
      <nav class="topo">
        <strong>Gestão de Colaboradores</strong>
        <a routerLink="/colaboradores" routerLinkActive="ativo">Colaboradores</a>
        <a routerLink="/unidades" routerLinkActive="ativo">Unidades</a>
        <a routerLink="/usuarios" routerLinkActive="ativo">Usuários</a>
        <button (click)="sair()">Sair</button>
      </nav>
    }
    <app-notificacoes />
    <main class="conteudo">
      <router-outlet />
    </main>
  `,
  styles: `
    .topo { display: flex; gap: 1rem; align-items: center; padding: .75rem 1.5rem; background: #fff; border-bottom: 1px solid #e3e6ea; }
    .topo a { text-decoration: none; } .topo a.ativo { font-weight: 600; }
    .topo button { margin-left: auto; }
    .conteudo { padding: 1.5rem; max-width: 960px; margin: 0 auto; }
  `
})
export class AppComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  sair(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
