import { Component, input, output } from '@angular/core';
import { ButtonModule } from 'carbon-components-angular/button';
import { ModalModule } from 'carbon-components-angular/modal';

/**
 * Modal de confirmação para ações destrutivas.
 *
 * Recebe o estado de aberto em vez de controlá-lo: quem dispara a ação é
 * quem sabe o que está prestes a ser removido, então a tela mantém o estado
 * e este componente só desenha e devolve a decisão.
 */
@Component({
  selector: 'app-confirmacao',
  imports: [ModalModule, ButtonModule],
  template: `
    <cds-modal [open]="aberto()" size="sm" theme="danger" (close)="cancelar.emit()">
      <cds-modal-header closeLabel="Fechar" (closeSelect)="cancelar.emit()">
        <h3 cdsModalHeaderHeading>{{ titulo() }}</h3>
      </cds-modal-header>

      <section cdsModalContent>
        <p cdsModalContentText>{{ mensagem() }}</p>
      </section>

      <cds-modal-footer>
        <button cdsButton="secondary" (click)="cancelar.emit()">Cancelar</button>
        <button cdsButton="danger" [disabled]="processando()" (click)="confirmar.emit()">
          {{ processando() ? 'Removendo…' : rotuloConfirmar() }}
        </button>
      </cds-modal-footer>
    </cds-modal>
  `
})
export class ConfirmacaoComponent {
  readonly aberto = input.required<boolean>();
  readonly titulo = input('Confirmar remoção');
  readonly mensagem = input.required<string>();
  readonly rotuloConfirmar = input('Remover');
  readonly processando = input(false);

  readonly confirmar = output<void>();
  readonly cancelar = output<void>();
}
