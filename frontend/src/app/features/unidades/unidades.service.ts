import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import {
  AtualizarParcialUnidade,
  AtualizarUnidade,
  CriarUnidade,
  Pagina,
  ParametrosPaginacao,
  Unidade
} from '../../core/models/modelos';
import { paramsDePaginacao } from '../../core/services/parametros';

@Injectable({ providedIn: 'root' })
export class UnidadesService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/v1/unidades';

  /** O GET já devolve os colaboradores de cada unidade — requisito do enunciado. */
  listar(paginacao?: ParametrosPaginacao): Observable<Pagina<Unidade>> {
    return this.http.get<Pagina<Unidade>>(this.base, { params: paramsDePaginacao(paginacao) });
  }

  obterPorId(id: string): Observable<Unidade> {
    return this.http.get<Unidade>(`${this.base}/${id}`);
  }

  criar(dto: CriarUnidade): Observable<Unidade> {
    return this.http.post<Unidade>(this.base, dto);
  }

  atualizar(id: string, dto: AtualizarUnidade): Observable<Unidade> {
    return this.http.put<Unidade>(`${this.base}/${id}`, dto);
  }

  /** É por aqui que se ativa/inativa uma unidade, enviando apenas `{ ativo }`. */
  atualizarParcial(id: string, dto: AtualizarParcialUnidade): Observable<Unidade> {
    return this.http.patch<Unidade>(`${this.base}/${id}`, dto);
  }
}
