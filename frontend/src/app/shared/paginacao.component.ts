import { Component, computed, input, output } from '@angular/core';
import { PaginationModel, PaginationModule } from 'carbon-components-angular/pagination';

/**
 * Rodapé de paginação.
 *
 * O cds-pagination do Carbon recebe um PaginationModel — um objeto mutável.
 * Aqui ele é reconstruído por um computed a partir dos signals de entrada,
 * para que a fonte da verdade continue sendo o estado da tela, e não o
 * objeto que a biblioteca guarda por dentro.
 */
@Component({
  selector: 'app-paginacao',
  imports: [PaginationModule],
  template: `
    @if (totalDePaginas() > 1) {
      <cds-pagination [model]="modelo()" (selectPage)="mudarPagina.emit($event)" />
    }
  `
})
export class PaginacaoComponent {
  readonly pagina = input.required<number>();
  readonly tamanho = input.required<number>();
  readonly total = input.required<number>();
  readonly totalDePaginas = input.required<number>();

  readonly mudarPagina = output<number>();

  readonly modelo = computed(() => {
    const modelo = new PaginationModel();
    modelo.currentPage = this.pagina();
    modelo.pageLength = this.tamanho();
    modelo.totalDataLength = this.total();

    return modelo;
  });
}
