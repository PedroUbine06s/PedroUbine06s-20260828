import { Component, effect, input, output } from '@angular/core';
import { PaginationModel, PaginationModule } from 'carbon-components-angular/pagination';

/** O que a listagem precisa saber quando a pessoa mexe na paginação. */
export interface MudancaDePagina {
  pagina: number;
  tamanho: number;
}

/**
 * Rodapé de paginação.
 *
 * O cds-pagination recebe um PaginationModel, que é um objeto mutável — e ao trocar o
 * tamanho da página o Carbon escreve nele e só então emite selectPage. Por isso o modelo
 * aqui é uma instância estável, sincronizada por efeito: recriá-lo a cada leitura, como um
 * computed faria, descartaria justamente a escrita que carrega o tamanho novo.
 */
@Component({
  selector: 'app-paginacao',
  imports: [PaginationModule],
  template: `
    <!-- Sempre visível, mesmo com uma página só: mostra o total de itens e o seletor de
         tamanho. Escondida, dava a impressão de que a listagem não era paginada. -->
    <cds-pagination [model]="modelo" (selectPage)="aoSelecionar($event)" />
  `
})
export class PaginacaoComponent {
  readonly pagina = input.required<number>();
  readonly tamanho = input.required<number>();
  readonly total = input.required<number>();

  readonly mudou = output<MudancaDePagina>();

  readonly modelo = new PaginationModel();

  constructor() {
    effect(() => {
      this.modelo.currentPage = this.pagina();
      this.modelo.pageLength = this.tamanho();
      this.modelo.totalDataLength = this.total();
    });
  }

  aoSelecionar(pagina: number): void {
    // O tamanho vem do modelo, não da entrada: quando a mudança foi no seletor de itens por
    // página, é lá que o valor novo está — a entrada ainda reflete a requisição anterior.
    this.mudou.emit({ pagina, tamanho: this.modelo.pageLength ?? this.tamanho() });
  }
}
