import { Component, inject } from '@angular/core';
import { NotificationModule } from 'carbon-components-angular/notification';
import { NotificacaoService } from '../core/services/notificacao.service';

/** Pilha de toasts do Carbon, alimentada pelo NotificacaoService. */
@Component({
  selector: 'app-notificacoes',
  imports: [NotificationModule],
  template: `
    <div class="pilha" role="status" aria-live="polite">
      @for (n of servico.notificacoes(); track n.id) {
        <cds-toast
          [notificationObj]="{
            type: n.tipo,
            title: n.titulo,
            subtitle: n.mensagem,
            caption: '',
            showClose: true
          }"
          (close)="servico.remover(n.id)"
        />
      }
    </div>
  `,
  styles: `
    .pilha {
      position: fixed;
      top: 1rem;
      right: 1rem;
      z-index: 9000;
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }
  `
})
export class NotificacoesComponent {
  readonly servico = inject(NotificacaoService);
}
