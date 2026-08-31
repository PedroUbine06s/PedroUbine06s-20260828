import { Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter, map } from 'rxjs';
import { UIShellModule } from 'carbon-components-angular/ui-shell';
import { AuthService } from './core/services/auth.service';
import { NotificacoesComponent } from './shared/notificacoes.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, UIShellModule, NotificacoesComponent],
  template: `
    @if (auth.estaLogado()) {
      <!-- brand vazio: o padrão do cds-header é "IBM", que não é a marca deste portal. -->
      <cds-header
        brand=""
        name="Gestão de Colaboradores"
        [route]="['/colaboradores']"
        [useRouter]="true">
        <cds-header-navigation>
          @for (item of navegacao; track item.rota) {
            <cds-header-item
              [route]="[item.rota]"
              [useRouter]="true"
              [isCurrentPage]="rotaAtiva() === item.rota">
              {{ item.rotulo }}
            </cds-header-item>
          }
        </cds-header-navigation>

        <cds-header-global>
          <button class="cds--header__action" type="button" (click)="sair()">Sair</button>
        </cds-header-global>
      </cds-header>
    }

    <app-notificacoes />

    <main class="conteudo" [class.com-cabecalho]="auth.estaLogado()">
      <router-outlet />
    </main>
  `,
  styles: `
    .conteudo { padding: 1.5rem; max-width: 72rem; margin: 0 auto; }
    .conteudo.com-cabecalho { margin-top: 3rem; }
    .cds--header__action { width: auto; padding: 0 1rem; }
  `
})
export class AppComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly navegacao = [
    { rota: '/colaboradores', rotulo: 'Colaboradores' },
    { rota: '/unidades', rotulo: 'Unidades' },
    { rota: '/usuarios', rotulo: 'Usuários' }
  ];

  private readonly url = toSignal(
    this.router.events.pipe(
      filter(evento => evento instanceof NavigationEnd),
      map(() => this.router.url)
    ),
    { initialValue: this.router.url }
  );

  /** Rota base atual, para marcar o item de navegação correspondente. */
  readonly rotaAtiva = computed(() => {
    const url = this.url();

    return this.navegacao.find(item => url.startsWith(item.rota))?.rota ?? '';
  });

  sair(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
