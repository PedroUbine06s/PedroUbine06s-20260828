import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AtualizarColaborador,
  Colaborador,
  CriarColaborador,
  Pagina,
  ParametrosPaginacao
} from '../../core/models/modelos';
import { paramsDePaginacao } from '../../core/services/parametros';

@Injectable({ providedIn: 'root' })
export class ColaboradoresService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/v1/colaboradores';

  listar(paginacao?: ParametrosPaginacao): Observable<Pagina<Colaborador>> {
    return this.http.get<Pagina<Colaborador>>(this.base, { params: paramsDePaginacao(paginacao) });
  }

  obterPorId(id: string): Observable<Colaborador> {
    return this.http.get<Colaborador>(`${this.base}/${id}`);
  }

  /** Unidade inativa recusa o cadastro com 422 — regra central do enunciado. */
  criar(dto: CriarColaborador): Observable<Colaborador> {
    return this.http.post<Colaborador>(this.base, dto);
  }

  atualizar(id: string, dto: AtualizarColaborador): Observable<Colaborador> {
    return this.http.put<Colaborador>(`${this.base}/${id}`, dto);
  }

  remover(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
