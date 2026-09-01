import { Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'carbon-components-angular/button';
import { LoadingModule } from 'carbon-components-angular/loading';
import { Pagina, Usuario } from '../../core/models/modelos';
import { MudancaDePagina, PaginacaoComponent } from '../../shared/paginacao.component';
import { StatusBadgeComponent } from '../../shared/status-badge.component';
import { UsuarioFormComponent } from './usuario-form.component';
import { paginaDaUrl, tamanhoDaUrl, TAMANHO_PADRAO } from '../../core/services/parametros';
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
          (click)="mudarFiltro(opcao.valor)"
        >
          {{ opcao.rotulo }}
        </button>
      }
    </div>

    @if (carregando()) {
      <cds-loading />
    } @else if (usuarios().length === 0) {
      <p class="vazio">Nenhum usuário encontrado para este filtro.</p>
    } @else {
      <div class="rolagem">
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
      </div>

      <app-paginacao
        [pagina]="dados()!.pagina"
        [tamanho]="tamanho()"
        [total]="dados()!.total"
        (mudou)="aoMudarPaginacao($event)"
      />
    }

    @if (formAberto()) {
      <app-usuario-form
        [aberto]="formAberto()"
        [usuario]="emEdicao()"
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
    .filtros {
      display: flex;
      gap: 0.25rem;
      margin-bottom: 1rem;
    }
    .filtros .selecionado {
      background: var(--cds-background-selected, #e0e0e0);
    }
    .vazio {
      color: var(--cds-text-secondary, #525252);
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
export class UsuariosListaComponent {
  private readonly service = inject(UsuariosService);

  readonly filtros: { valor: Filtro; rotulo: string }[] = [
    { valor: 'todos', rotulo: 'Todos' },
    { valor: 'ativos', rotulo: 'Ativos' },
    { valor: 'inativos', rotulo: 'Inativos' }
  ];

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly dados = signal<Pagina<Usuario> | null>(null);
  readonly carregando = signal(true);

  private readonly parametros = toSignal(this.route.queryParamMap, {
    initialValue: this.route.snapshot.queryParamMap
  });

  /** Página e filtro vivem na URL: a listagem filtrada vira um link compartilhável. */
  readonly pagina = computed(() => paginaDaUrl(this.parametros().get('pagina')));

  /** O tamanho também vive na URL, senão trocá-lo se perderia ao virar a página. */
  readonly tamanho = computed(() => tamanhoDaUrl(this.parametros().get('tamanho')));

  readonly filtro = computed<Filtro>(() => {
    const valor = this.parametros().get('status');

    return valor === 'ativos' || valor === 'inativos' ? valor : 'todos';
  });

  readonly formAberto = signal(false);
  readonly emEdicao = signal<Usuario | null>(null);

  readonly usuarios = computed(() => this.dados()?.itens ?? []);

  constructor() {
    effect(() => {
      this.pagina();
      this.tamanho();
      this.filtro();
      this.carregar();
    });
  }

  /** Trocar de filtro volta à primeira página: a numeração antiga não vale mais. */
  mudarFiltro(filtro: Filtro): void {
    this.navegar({ status: filtro === 'todos' ? null : filtro, pagina: null });
  }

  /** Página 1 e tamanho padrão saem da URL, para não sujar o link com o que já é padrão. */
  aoMudarPaginacao({ pagina, tamanho }: MudancaDePagina): void {
    this.navegar({
      pagina: pagina === 1 ? null : pagina,
      tamanho: tamanho === TAMANHO_PADRAO ? null : tamanho
    });
  }

  private navegar(queryParams: Record<string, string | number | null>): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'merge'
    });
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

    this.service.listar({ ativo, pagina: this.pagina(), tamanho: this.tamanho() }).subscribe({
      next: pagina => {
        this.dados.set(pagina);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false)
    });
  }
}
