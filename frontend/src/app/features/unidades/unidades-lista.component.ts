import { Component, computed, inject, signal } from '@angular/core';
import { ButtonModule } from 'carbon-components-angular/button';
import { LoadingModule } from 'carbon-components-angular/loading';
import { Pagina, Unidade } from '../../core/models/modelos';
import { NotificacaoService } from '../../core/services/notificacao.service';
import { PaginacaoComponent } from '../../shared/paginacao.component';
import { StatusBadgeComponent } from '../../shared/status-badge.component';
import { UnidadeFormComponent } from './unidade-form.component';
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
      <table class="cds--data-table cds--data-table--md">
        <thead>
          <tr>
            <th></th>
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
              <td class="coluna-expandir">
                <button
                  cdsButton="ghost"
                  size="sm"
                  [attr.aria-expanded]="expandida() === u.id"
                  [attr.aria-label]="
                    (expandida() === u.id ? 'Recolher' : 'Expandir') + ' colaboradores de ' + u.nome
                  "
                  (click)="alternarExpansao(u.id)">
                  {{ expandida() === u.id ? '▾' : '▸' }}
                </button>
              </td>
              <td>{{ u.codigo }}</td>
              <td>{{ u.nome }}</td>
              <td><app-status-badge [ativo]="u.ativo" /></td>
              <td>{{ u.colaboradores.length }}</td>
              <td class="acoes">
                <button cdsButton="ghost" size="sm" (click)="abrirEdicao(u)">Editar</button>
                <button
                  cdsButton="ghost"
                  size="sm"
                  [disabled]="alternandoStatus() === u.id"
                  (click)="alternarStatus(u)">
                  {{ u.ativo ? 'Inativar' : 'Ativar' }}
                </button>
              </td>
            </tr>

            @if (expandida() === u.id) {
              <tr class="linha-detalhe">
                <td colspan="6">
                  @if (u.colaboradores.length === 0) {
                    <p class="vazio">Esta unidade não tem colaboradores.</p>
                  } @else {
                    <ul class="colaboradores">
                      @for (c of u.colaboradores; track c.id) {
                        <li><strong>{{ c.codigo }}</strong> — {{ c.nome }}</li>
                      }
                    </ul>
                  }
                </td>
              </tr>
            }
          }
        </tbody>
      </table>

      <app-paginacao
        [pagina]="dados()!.pagina"
        [tamanho]="dados()!.tamanho"
        [total]="dados()!.total"
        [totalDePaginas]="dados()!.totalDePaginas"
        (mudarPagina)="irParaPagina($event)" />
    }

    @if (formAberto()) {
      <app-unidade-form
        [aberto]="formAberto()"
        [unidade]="emEdicao()"
        (salvo)="aoSalvar()"
        (cancelado)="fecharForm()" />
    }
  `,
  styles: `
    .cabecalho { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
    .cabecalho h1 { margin: 0; }
    .vazio { color: var(--cds-text-secondary, #525252); margin: 0; }
    .acoes { display: flex; gap: .25rem; }
    .coluna-expandir { width: 2.5rem; }
    .linha-detalhe td { background: var(--cds-layer-accent, #e0e0e0); }
    .colaboradores { margin: 0; padding-left: 1.25rem; display: grid; gap: .25rem; }
    table { width: 100%; }
  `
})
export class UnidadesListaComponent {
  private readonly service = inject(UnidadesService);
  private readonly notificacao = inject(NotificacaoService);

  readonly dados = signal<Pagina<Unidade> | null>(null);
  readonly carregando = signal(true);
  readonly pagina = signal(1);

  /** Id da unidade com a linha de colaboradores aberta, se houver. */
  readonly expandida = signal<string | null>(null);
  readonly alternandoStatus = signal<string | null>(null);

  readonly formAberto = signal(false);
  readonly emEdicao = signal<Unidade | null>(null);

  readonly unidades = computed(() => this.dados()?.itens ?? []);

  constructor() {
    this.carregar();
  }

  alternarExpansao(id: string): void {
    this.expandida.update(atual => (atual === id ? null : id));
  }

  irParaPagina(pagina: number): void {
    this.pagina.set(pagina);
    this.carregar();
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
