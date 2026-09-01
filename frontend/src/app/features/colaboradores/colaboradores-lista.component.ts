import { Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'carbon-components-angular/button';
import { LoadingModule } from 'carbon-components-angular/loading';
import { Colaborador, Pagina } from '../../core/models/modelos';
import { NotificacaoService } from '../../core/services/notificacao.service';
import { ConfirmacaoComponent } from '../../shared/confirmacao.component';
import { PaginacaoComponent } from '../../shared/paginacao.component';
import { ColaboradorFormComponent } from './colaborador-form.component';
import { paginaDaUrl } from '../../core/services/parametros';
import { ColaboradoresService } from './colaboradores.service';

@Component({
  selector: 'app-colaboradores-lista',
  imports: [
    ButtonModule,
    LoadingModule,
    PaginacaoComponent,
    ColaboradorFormComponent,
    ConfirmacaoComponent
  ],
  template: `
    <header class="cabecalho">
      <h1>Colaboradores</h1>
      <button cdsButton="primary" (click)="abrirCriacao()">Novo colaborador</button>
    </header>

    @if (carregando()) {
      <cds-loading />
    } @else if (colaboradores().length === 0) {
      <p class="vazio">Nenhum colaborador cadastrado.</p>
    } @else {
      <div class="rolagem">
        <table class="cds--data-table cds--data-table--md">
          <thead>
            <tr>
              <th><span class="cds--table-header-label">Código</span></th>
              <th><span class="cds--table-header-label">Nome</span></th>
              <th><span class="cds--table-header-label">Unidade</span></th>
              <th><span class="cds--table-header-label">Usuário</span></th>
              <th><span class="cds--table-header-label">Ações</span></th>
            </tr>
          </thead>
          <tbody>
            @for (c of colaboradores(); track c.id) {
              <tr>
                <td>{{ c.codigo }}</td>
                <td>{{ c.nome }}</td>
                <td>
                  {{ c.nomeUnidade }} <span class="codigo">{{ c.codigoUnidade }}</span>
                </td>
                <td>
                  {{ c.loginUsuario }} <span class="codigo">{{ c.codigoUsuario }}</span>
                </td>
                <td class="acoes">
                  <button cdsButton="ghost" size="sm" (click)="abrirEdicao(c)">Editar</button>
                  <button cdsButton="danger--ghost" size="sm" (click)="pedirRemocao(c)">
                    Remover
                  </button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <app-paginacao
        [pagina]="dados()!.pagina"
        [tamanho]="dados()!.tamanho"
        [total]="dados()!.total"
        (mudarPagina)="irParaPagina($event)"
      />
    }

    @if (formAberto()) {
      <app-colaborador-form
        [aberto]="formAberto()"
        [colaborador]="emEdicao()"
        (salvo)="aoSalvar()"
        (cancelado)="fecharForm()"
      />
    }

    @if (emRemocao(); as alvo) {
      <app-confirmacao
        [aberto]="true"
        [mensagem]="
          'Remover ' + alvo.nome + ' (' + alvo.codigo + ')? Esta ação não pode ser desfeita.'
        "
        [processando]="removendo()"
        (confirmar)="confirmarRemocao()"
        (cancelar)="emRemocao.set(null)"
      />
    }
  `,
  styles: `
    .cabecalho {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 1rem;
    }
    .cabecalho h1 {
      margin: 0;
    }
    .vazio {
      color: var(--cds-text-secondary, #525252);
    }
    .acoes {
      display: flex;
      gap: 0.25rem;
    }
    .codigo {
      color: var(--cds-text-secondary, #525252);
      font-size: 0.75rem;
    }
    /* A tabela rola dentro do próprio container em telas estreitas. Sem isto, ela
       empurra a página inteira e o cabeçalho e o título saem da vista. */
    .rolagem {
      overflow-x: auto;
    }
    table {
      width: 100%;
      min-width: 34rem;
    }
  `
})
export class ColaboradoresListaComponent {
  private readonly service = inject(ColaboradoresService);
  private readonly notificacao = inject(NotificacaoService);

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly dados = signal<Pagina<Colaborador> | null>(null);
  readonly carregando = signal(true);

  private readonly parametros = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap
  });

  /**
   * A página vive na URL, não num signal local: recarregar, voltar pelo navegador e
   * compartilhar o link preservam o lugar na listagem.
   */
  readonly pagina = computed(() => paginaDaUrl(this.parametros().get('pagina')));

  readonly formAberto = signal(false);
  readonly emEdicao = signal<Colaborador | null>(null);

  readonly emRemocao = signal<Colaborador | null>(null);
  readonly removendo = signal(false);

  readonly colaboradores = computed(() => this.dados()?.itens ?? []);

  constructor() {
    // Recarrega sempre que a página da URL muda — inclusive pelo botão voltar.
    effect(() => {
      this.pagina();
      this.carregar();
    });
  }

  irParaPagina(pagina: number): void {
    this.navegarParaPagina(pagina);
  }

  /** Página 1 sai da URL para não sujar o link com o valor padrão. */
  private navegarParaPagina(pagina: number): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { pagina: pagina === 1 ? null : pagina },
      queryParamsHandling: 'merge'
    });
  }

  abrirCriacao(): void {
    this.emEdicao.set(null);
    this.formAberto.set(true);
  }

  abrirEdicao(colaborador: Colaborador): void {
    this.emEdicao.set(colaborador);
    this.formAberto.set(true);
  }

  fecharForm(): void {
    this.formAberto.set(false);
    this.emEdicao.set(null);
  }

  aoSalvar(): void {
    this.fecharForm();
    this.carregar();
  }

  pedirRemocao(colaborador: Colaborador): void {
    this.emRemocao.set(colaborador);
  }

  confirmarRemocao(): void {
    const alvo = this.emRemocao();
    if (!alvo) return;

    this.removendo.set(true);

    this.service.remover(alvo.id).subscribe({
      next: () => {
        this.notificacao.sucesso(`${alvo.nome} foi removido.`);
        this.removendo.set(false);
        this.emRemocao.set(null);

        // Ao esvaziar a última página, recua uma para não exibir uma lista vazia.
        // Navegar já dispara o efeito que recarrega; sem isso, recarrega aqui mesmo.
        if (this.colaboradores().length === 1 && this.pagina() > 1) {
          this.navegarParaPagina(this.pagina() - 1);
        } else {
          this.carregar();
        }
      },
      error: () => {
        this.removendo.set(false);
        this.emRemocao.set(null);
      }
    });
  }

  private carregar(): void {
    this.carregando.set(true);

    this.service.listar({ pagina: this.pagina() }).subscribe({
      next: pagina => {
        this.dados.set(pagina);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false)
    });
  }
}
