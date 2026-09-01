import { Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'carbon-components-angular/button';
import { LoadingModule } from 'carbon-components-angular/loading';
import { Pagina, Unidade } from '../../core/models/modelos';
import { NotificacaoService } from '../../core/services/notificacao.service';
import { PaginacaoComponent } from '../../shared/paginacao.component';
import { StatusBadgeComponent } from '../../shared/status-badge.component';
import { UnidadeFormComponent } from './unidade-form.component';
import { paginaDaUrl } from '../../core/services/parametros';
import { UnidadesService } from './unidades.service';

@Component({
  selector: 'app-unidades-lista',
  imports: [
    ButtonModule,
    LoadingModule,
    StatusBadgeComponent,
    PaginacaoComponent,
    UnidadeFormComponent
  ],
  template: `
    <header class="cabecalho">
      <h1>Unidades</h1>
      <button cdsButton="primary" (click)="abrirCriacao()">Nova unidade</button>
    </header>

    @if (carregando()) {
      <cds-loading />
    } @else if (unidades().length === 0) {
      <p class="vazio">Nenhuma unidade cadastrada.</p>
    } @else {
      <div class="rolagem">
        <table class="cds--data-table cds--data-table--md">
          <thead>
            <tr>
              <th><span class="cds--table-header-label">Código</span></th>
              <th><span class="cds--table-header-label">Nome</span></th>
              <th><span class="cds--table-header-label">Status</span></th>
              <th><span class="cds--table-header-label">Colaboradores</span></th>
              <th><span class="cds--table-header-label">Ações</span></th>
            </tr>
          </thead>
          <tbody>
            @for (u of unidades(); track u.id) {
              <tr>
                <td>{{ u.codigo }}</td>
                <td>{{ u.nome }}</td>
                <td><app-status-badge [ativo]="u.ativo" /></td>
                <td>
                  <!-- O rótulo em palavras é o que faz a expansão ser descoberta; o chevron
                     sozinho não dizia que ali havia uma ação. Sem colaboradores não há o
                     que expandir, então vira texto simples. -->
                  @if (u.colaboradores.length > 0) {
                    <button
                      class="expandir"
                      type="button"
                      [attr.aria-expanded]="expandida() === u.id"
                      (click)="alternarExpansao(u.id)"
                    >
                      <span class="chevron" [class.aberto]="expandida() === u.id">⌄</span>
                      {{ u.colaboradores.length }}
                      {{ u.colaboradores.length === 1 ? 'colaborador' : 'colaboradores' }}
                    </button>
                  } @else {
                    <span class="vazio">nenhum colaborador</span>
                  }
                </td>
                <td class="acoes">
                  <button cdsButton="ghost" size="sm" (click)="abrirEdicao(u)">Editar</button>
                  <button
                    cdsButton="ghost"
                    size="sm"
                    [disabled]="alternandoStatus() === u.id"
                    (click)="alternarStatus(u)"
                  >
                    {{ u.ativo ? 'Inativar' : 'Ativar' }}
                  </button>
                </td>
              </tr>

              @if (expandida() === u.id) {
                <tr class="linha-detalhe">
                  <td colspan="5">
                    @if (u.colaboradores.length === 0) {
                      <p class="vazio">Esta unidade não tem colaboradores.</p>
                    } @else {
                      <ul class="colaboradores">
                        @for (c of u.colaboradores; track c.id) {
                          <li>
                            <strong>{{ c.codigo }}</strong> — {{ c.nome }}
                          </li>
                        }
                      </ul>
                    }
                  </td>
                </tr>
              }
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
      <app-unidade-form
        [aberto]="formAberto()"
        [unidade]="emEdicao()"
        (salvo)="aoSalvar()"
        (cancelado)="fecharForm()"
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
      margin: 0;
    }
    .acoes {
      display: flex;
      gap: 0.25rem;
    }
    .expandir {
      display: inline-flex;
      align-items: center;
      gap: 0.4rem;
      padding: 0;
      border: 0;
      background: none;
      font: inherit;
      color: var(--cds-link-primary, #0f62fe);
      cursor: pointer;
    }
    .expandir:hover {
      text-decoration: underline;
    }
    .expandir:focus-visible {
      outline: 2px solid var(--cds-focus, #0f62fe);
      outline-offset: 2px;
    }
    .chevron {
      display: inline-block;
      transition: transform 0.15s ease;
      line-height: 1;
    }
    .chevron.aberto {
      transform: rotate(180deg);
    }
    .vazio {
      color: var(--cds-text-secondary, #525252);
    }
    .linha-detalhe td {
      background: var(--cds-layer-accent, #e0e0e0);
    }
    .colaboradores {
      margin: 0;
      padding-left: 1.25rem;
      display: grid;
      gap: 0.25rem;
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
export class UnidadesListaComponent {
  private readonly service = inject(UnidadesService);
  private readonly notificacao = inject(NotificacaoService);

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly dados = signal<Pagina<Unidade> | null>(null);
  readonly carregando = signal(true);

  private readonly parametros = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap
  });

  /** A página vive na URL: F5 e o botão voltar preservam o lugar na listagem. */
  readonly pagina = computed(() => paginaDaUrl(this.parametros().get('pagina')));

  /** Id da unidade com a linha de colaboradores aberta, se houver. */
  readonly expandida = signal<string | null>(null);
  readonly alternandoStatus = signal<string | null>(null);

  readonly formAberto = signal(false);
  readonly emEdicao = signal<Unidade | null>(null);

  readonly unidades = computed(() => this.dados()?.itens ?? []);

  constructor() {
    effect(() => {
      this.pagina();
      this.carregar();
    });
  }

  alternarExpansao(id: string): void {
    this.expandida.update(atual => (atual === id ? null : id));
  }

  irParaPagina(pagina: number): void {
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

  abrirEdicao(unidade: Unidade): void {
    this.emEdicao.set(unidade);
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

  /** Usa PATCH: só o campo `ativo` é enviado, o nome permanece como está. */
  alternarStatus(unidade: Unidade): void {
    this.alternandoStatus.set(unidade.id);

    this.service.atualizarParcial(unidade.id, { ativo: !unidade.ativo }).subscribe({
      next: () => {
        this.notificacao.sucesso(
          unidade.ativo
            ? `${unidade.nome} foi inativada e não aceita mais novos colaboradores.`
            : `${unidade.nome} foi reativada.`
        );
        this.alternandoStatus.set(null);
        this.carregar();
      },
      error: () => this.alternandoStatus.set(null)
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
