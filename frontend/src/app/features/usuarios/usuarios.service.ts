import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AtualizarUsuario,
  CriarUsuario,
  FiltroUsuarios,
  Pagina,
  Usuario
} from '../../core/models/modelos';
import { paramsDePaginacao } from '../../core/services/parametros';

@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/v1/usuarios';

  /** Campo indefinido = sem filtro; o backend só filtra quando o parâmetro vem. */
  listar(filtro?: FiltroUsuarios): Observable<Pagina<Usuario>> {
    let params = paramsDePaginacao(filtro);

    if (filtro?.ativo !== undefined) params = params.set('ativo', filtro.ativo);
    if (filtro?.semColaborador !== undefined) {
      params = params.set('semColaborador', filtro.semColaborador);
    }

    return this.http.get<Pagina<Usuario>>(this.base, { params });
  }

  obterPorId(id: string): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.base}/${id}`);
  }

  /** O código é gerado pelo sistema (USR000001). Senha mínima de 8 caracteres. */
  criar(dto: CriarUsuario): Observable<Usuario> {
    return this.http.post<Usuario>(this.base, dto);
  }

  /** Contrato restritivo: só senha e status. Senha ausente = não alterar. */
  atualizar(id: string, dto: AtualizarUsuario): Observable<Usuario> {
    return this.http.put<Usuario>(`${this.base}/${id}`, dto);
  }
}
