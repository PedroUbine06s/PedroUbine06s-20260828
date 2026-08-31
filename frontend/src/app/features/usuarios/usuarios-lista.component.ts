import { Component, computed, inject, signal } from '@angular/core';
import { ButtonModule } from 'carbon-components-angular/button';
import { LoadingModule } from 'carbon-components-angular/loading';
import { Pagina, Usuario } from '../../core/models/modelos';
import { PaginacaoComponent } from '../../shared/paginacao.component';
import { StatusBadgeComponent } from '../../shared/status-badge.component';
import { UsuarioFormComponent } from './usuario-form.component';
import { UsuariosService } from './usuarios.service';

type Filtro = 'todos' | 'ativos' | 'inativos';

@Component({
  selector: 'app-usuarios-lista',
  imports: [
    ButtonModule,
    LoadingModule,
    StatusBadgeComponent,
    PaginacaoComponent,
    UsuarioFormComponent
  ],
  template: `
    <header class="cabecalho">
      <h1>Usuários</h1>
      <button cdsButton="primary" (click)="abrirCriacao()">Novo usuário</button>
    </header>

    <div class="filtros" role="group" aria-label="Filtrar por status">
      @for (opcao of filtros; track opcao.valor) {
        <button
          cdsButton="ghost"
          [class.selecionado]="filtro() === opcao.valor"
          (click)="mudarFiltro(opcao.valor)">
          {{ opcao.rotulo }}
        </button>
      }
    </div>

    @if (carregando()) {
      <cds-loading />
    } @else if (usuarios().length === 0) {
      <p class="vazio">Nenhum usuário encontrado para este filtro.</p>
    } @else {
      <table class="cds--data-table cds--data-table--md">
        <thead>
          <tr>
            <th><span class="cds--table-header-label">Código</span></th>
            <th><span class="cds--table-header-label">Login</span></th>
            <th><span class="cds--table-header-label">Status</span></th>
            <th><span class="cds--table-header-label">Ações</span></th>
          </tr>
        </thead>
        <tbody>
          @for (u of usuarios(); track u.id) {
            <tr>
              <td>{{ u.codigo }}</td>
              <td>{{ u.login }}</td>
              <td><app-status-badge [ativo]="u.ativo" /></td>
              <td>
                <button cdsButton="ghost" size="sm" (click)="abrirEdicao(u)">Editar</button>
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
      <app-usuario-form
        [aberto]="formAberto()"
        [usuario]="emEdicao()"
        (salvo)="aoSalvar()"
        (cancelado)="fecharForm()" />
    }
  `,
  styles: `
    .cabecalho { display: flex; align-items: center; justify-content: space-between; margin-bottom: 1rem; }
    .cabecalho h1 { margin: 0; }
    .filtros { display: flex; gap: .25rem; margin-bottom: 1rem; }
    .filtros .selecionado { background: var(--cds-background-selected, #e0e0e0); }
    .vazio { color: var(--cds-text-secondary, #525252); }
    table { width: 100%; }
  `
})
export class UsuariosListaComponent {
  private readonly service = inject(UsuariosService);

  readonly filtros: { valor: Filtro; rotulo: string }[] = [
    { valor: 'todos', rotulo: 'Todos' },
    { valor: 'ativos', rotulo: 'Ativos' },
    { valor: 'inativos', rotulo: 'Inativos' }
  ];

  readonly dados = signal<Pagina<Usuario> | null>(null);
  readonly carregando = signal(true);
  readonly filtro = signal<Filtro>('todos');
  readonly pagina = signal(1);

  readonly formAberto = signal(false);
  readonly emEdicao = signal<Usuario | null>(null);

  readonly usuarios = computed(() => this.dados()?.itens ?? []);

  constructor() {
    this.carregar();
  }

  mudarFiltro(filtro: Filtro): void {
    this.filtro.set(filtro);
    this.pagina.set(1);
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

  abrirEdicao(usuario: Usuario): void {
    this.emEdicao.set(usuario);
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

  private carregar(): void {
    this.carregando.set(true);

    const filtro = this.filtro();
    // 'todos' omite o parâmetro: a API só filtra quando ?ativo= é enviado.
    const ativo = filtro === 'todos' ? undefined : filtro === 'ativos';

    this.service.listar({ ativo, pagina: this.pagina() }).subscribe({
      next: pagina => {
        this.dados.set(pagina);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false)
    });
  }
}
