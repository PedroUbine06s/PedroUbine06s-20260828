import { Component, computed, inject, signal } from '@angular/core';
import { ButtonModule } from 'carbon-components-angular/button';
import { LoadingModule } from 'carbon-components-angular/loading';
import { Colaborador, Pagina } from '../../core/models/modelos';
import { NotificacaoService } from '../../core/services/notificacao.service';
import { ConfirmacaoComponent } from '../../shared/confirmacao.component';
import { PaginacaoComponent } from '../../shared/paginacao.component';
import { ColaboradorFormComponent } from './colaborador-form.component';
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
      <table class="cds--data-table cds--data-table--md">
        <thead>
          <tr>
            <th><span class="cds--table-header-label">Código</span></th>
            <th><span class="cds--table-header-label">Nome</span></th>
            <th><span class="cds--table-header-label">Unidade</span></th>
            <th><span class="cds--table-header-label">Ações</span></th>
          </tr>
        </thead>
        <tbody>
          @for (c of colaboradores(); track c.id) {
            <tr>
              <td>{{ c.codigo }}</td>
              <td>{{ c.nome }}</td>
              <td>{{ c.nomeUnidade }} <span class="codigo">{{ c.codigoUnidade }}</span></td>
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

      <app-paginacao
        [pagina]="dados()!.pagina"
        [tamanho]="dados()!.tamanho"
        [total]="dados()!.total"
        [totalDePaginas]="dados()!.totalDePaginas"
        (mudarPagina)="irParaPagina($event)" />
    }

    @if (formAberto()) {
      <app-colaborador-form
        [aberto]="formAberto()"
        [colaborador]="emEdicao()"
        (salvo)="aoSalvar()"
        (cancelado)="fecharForm()" />
    }

    @if (emRemocao(); as alvo) {
      <app-confirmacao
        [aberto]="true"
        [mensagem]="'Remover ' + alvo.nome + ' (' + alvo.codigo + ')? Esta ação não pode ser desfeita.'"
        [processando]="removendo()"
        (confirmar)="confirmarRemocao()"
        (cancelar)="emRemocao.set(null)" />
    }
  `,
  styles: `
    .cabecalho { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
    .cabecalho h1 { margin: 0; }
    .vazio { color: var(--cds-text-secondary, #525252); }
    .acoes { display: flex; gap: .25rem; }
    .codigo { color: var(--cds-text-secondary, #525252); font-size: .75rem; }
    table { width: 100%; }
  `
})
export class ColaboradoresListaComponent {
  private readonly service = inject(ColaboradoresService);
  private readonly notificacao = inject(NotificacaoService);

  readonly dados = signal<Pagina<Colaborador> | null>(null);
  readonly carregando = signal(true);
  readonly pagina = signal(1);

  readonly formAberto = signal(false);
  readonly emEdicao = signal<Colaborador | null>(null);

  readonly emRemocao = signal<Colaborador | null>(null);
  readonly removendo = signal(false);

  readonly colaboradores = computed(() => this.dados()?.itens ?? []);

  constructor() {
    this.carregar();
  }

  irParaPagina(pagina: number): void {
    this.pagina.set(pagina);
    this.carregar();
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
        if (this.colaboradores().length === 1 && this.pagina() > 1) {
          this.pagina.update(p => p - 1);
        }

        this.carregar();
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
