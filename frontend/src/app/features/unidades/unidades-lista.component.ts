import { Component } from '@angular/core';

@Component({
  selector: 'app-unidades-lista',
  template: `
    <h1>Unidades</h1>
    <p>TODO — seguir o padrão de ColaboradoresListaComponent, com:</p>
    <ul>
      <li>UnidadesService (o GET já traz os colaboradores de cada unidade — requisito)</li>
      <li>Linha expansível ou seção mostrando os colaboradores da unidade</li>
      <li>StatusBadgeComponent + ação de inativar/ativar</li>
      <li>Ao inativar: refletir que a unidade some do select de novo colaborador</li>
    </ul>
  `
})
export class UnidadesListaComponent {}
