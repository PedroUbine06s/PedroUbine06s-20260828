import { Component, computed, inject, signal } from '@angular/core';
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
        [useRouter]="true"
      >
        <!-- O Carbon esconde a navegação abaixo de 66rem e espera que ela apareça dentro
             do side nav. O hambúrguer é quem abre; a própria lib o esconde nas telas largas,
             onde a navegação do cabeçalho já aparece. -->
        <cds-hamburger [active]="menuAberto()" (selected)="menuAberto.update(aberto => !aberto)" />

        <cds-header-navigation>
          @for (item of navegacao; track item.rota) {
            <!-- activeLinkClass não tem valor padrão no cds-header-item e é repassado
                 direto ao routerLinkActive, que faz split() nele: sem isso o Angular
                 lança TypeError a cada item do menu no carregamento da página. -->
            <cds-header-item
              [route]="[item.rota]"
              [useRouter]="true"
              activeLinkClass="cds--header__menu-item--current"
              [isCurrentPage]="rotaAtiva() === item.rota"
            >
              {{ item.rotulo }}
            </cds-header-item>
          }
        </cds-header-navigation>

        <cds-header-global>
          <button class="cds--header__action" type="button" (click)="sair()">Sair</button>
        </cds-header-global>
      </cds-header>

      <!-- Mesma lista de rotas, para telas estreitas. Fechar ao navegar é obrigatório: o
           roteamento não recarrega a página, então o painel ficaria aberto por cima. -->
      <!-- hidden fecha o trilho recolhido de 48px que o Carbon deixa por cima do
           conteúdo quando o painel está fechado. -->
      <cds-sidenav [expanded]="menuAberto()" [hidden]="!menuAberto()">
        @for (item of navegacao; track item.rota) {
          <cds-sidenav-item
            [route]="[item.rota]"
            [useRouter]="true"
            [active]="rotaAtiva() === item.rota"
            (click)="menuAberto.set(false)"
          >
            {{ item.rotulo }}
          </cds-sidenav-item>
        }
      </cds-sidenav>
    }

    <app-notificacoes />

    <main class="conteudo" [class.com-cabecalho]="auth.estaLogado()">
      <router-outlet />
    </main>
  `,
  styles: `
    .conteudo {
      padding: 1.5rem;
      max-width: 72rem;
      margin: 0 auto;
    }
    .conteudo.com-cabecalho {
      margin-top: 3rem;
    }
    /* A classe do Carbon é feita para um ícone, que ela centraliza por outros meios.
       Com texto puro o conteúdo encostava no topo da caixa de 48px. */
    .cds--header__action {
      width: auto;
      padding: 0 1rem;
      align-items: center;
      justify-content: center;
    }
  `
})
export class AppComponent {
  readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  /** Só tem efeito abaixo de 66rem, onde a navegação do cabeçalho fica escondida. */
  readonly menuAberto = signal(false);

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
