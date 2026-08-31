import { Component, inject, signal } from '@angular/core';
import { Colaborador } from '../../core/models/modelos';
import { ColaboradoresService } from './colaboradores.service';

/** LISTAGEM DE REFERÊNCIA — estados de loading e lista vazia inclusos. */
@Component({
  selector: 'app-colaboradores-lista',
  template: `
    <h1>Colaboradores</h1>

    @if (carregando()) {
      <p>Carregando…</p>
    } @else if (colaboradores().length === 0) {
      <p>Nenhum colaborador cadastrado.</p>
    } @else {
      <table>
        <thead>
          <tr><th>Código</th><th>Nome</th><th>Unidade</th><th></th></tr>
        </thead>
        <tbody>
          @for (c of colaboradores(); track c.id) {
            <tr>
              <td>{{ c.codigo }}</td>
              <td>{{ c.nome }}</td>
              <td>{{ c.nomeUnidade }}</td>
              <td><!-- TODO: editar / remover (com confirm-dialog) --></td>
            </tr>
          }
        </tbody>
      </table>
    }

    <!-- TODO: botão "Novo colaborador" → form com select listando APENAS unidades ativas -->
  `,
  styles: `
    table { width: 100%; border-collapse: collapse; background: #fff; }
    th, td { text-align: left; padding: .6rem .8rem; border-bottom: 1px solid #edf0f3; }
  `
})
export class ColaboradoresListaComponent {
  private readonly service = inject(ColaboradoresService);

  readonly colaboradores = signal<Colaborador[]>([]);
  readonly carregando = signal(true);

  constructor() {
    this.service.listar().subscribe({
      next: pagina => {
        this.colaboradores.set(pagina.itens);
        this.carregando.set(false);
      },
      error: () => this.carregando.set(false)
    });
  }
}
