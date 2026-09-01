import { Component, computed, input } from '@angular/core';
import { TagModule } from 'carbon-components-angular/tag';

@Component({
  selector: 'app-status-badge',
  imports: [TagModule],
  template: `<cds-tag [type]="ativo() ? 'green' : 'gray'">{{ rotulo() }}</cds-tag>`
})
export class StatusBadgeComponent {
  readonly ativo = input.required<boolean>();

  readonly rotulo = computed(() => (this.ativo() ? 'Ativo' : 'Inativo'));
}
