import { Component, input } from '@angular/core';

/** Componente compartilhado: badge visual de status usado em todas as listagens. */
@Component({
  selector: 'app-status-badge',
  template: `<span class="badge" [class.ativo]="ativo()">{{ ativo() ? 'Ativo' : 'Inativo' }}</span>`,
  styles: `
    .badge { padding: .15rem .6rem; border-radius: 1rem; font-size: .8rem; background: #fde3e3; color: #a12020; }
    .badge.ativo { background: #e0f4e4; color: #1c7c34; }
  `
})
export class StatusBadgeComponent {
  readonly ativo = input.required<boolean>();
}
