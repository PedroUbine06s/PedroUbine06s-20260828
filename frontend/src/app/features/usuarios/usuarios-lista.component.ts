import { Component } from '@angular/core';

@Component({
  selector: 'app-usuarios-lista',
  template: `
    <h1>Usuários</h1>
    <p>TODO — seguir o padrão de ColaboradoresListaComponent, com:</p>
    <ul>
      <li>UsuariosService (listar aceita filtro <code>?ativo=</code> — requisito do enunciado)</li>
      <li>Filtro por status na tela (todos / ativos / inativos)</li>
      <li>StatusBadgeComponent na coluna de status</li>
      <li>Form de edição oferecendo SOMENTE senha e status (espelho do contrato da API)</li>
    </ul>
  `
})
export class UsuariosListaComponent {}
