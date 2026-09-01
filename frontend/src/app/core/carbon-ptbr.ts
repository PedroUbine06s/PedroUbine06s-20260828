import { provideAppInitializer, inject } from '@angular/core';
import { I18n } from 'carbon-components-angular/i18n';

/**
 * O Carbon traz os rótulos dos componentes em inglês. Traduzir aqui, uma vez, mantém os
 * textos num lugar só — a alternativa era repetir `[translations]` em cada uso.
 *
 * Só as chaves que aparecem na tela: o resto continua no padrão da biblioteca.
 */
export const carbonEmPortugues = provideAppInitializer(() => {
  inject(I18n).set({
    PAGINATION: {
      ITEMS_PER_PAGE: 'Itens por página:',
      OPEN_LIST_OF_OPTIONS: 'Abrir lista de opções',
      BACKWARD: 'Anterior',
      FORWARD: 'Próxima',
      TOTAL_ITEMS_UNKNOWN: '{{start}}-{{end}} itens',
      TOTAL_ITEMS: '{{start}}-{{end}} de {{total}} itens',
      TOTAL_ITEM: '{{start}}-{{end}} de {{total}} item',
      PAGE: 'página',
      OF_LAST_PAGES: 'de {{last}} páginas',
      OF_LAST_PAGE: 'de {{last}} página',
      NEXT: 'Próxima',
      PREVIOUS: 'Anterior',
      SELECT_ARIA: 'Selecione o número da página'
    },
    MODAL: {
      CLOSE: 'Fechar'
    }
  });
});
