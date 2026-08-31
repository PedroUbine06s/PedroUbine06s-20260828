import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Colaborador } from '../../core/models/modelos';

@Injectable({ providedIn: 'root' })
export class ColaboradoresService {
  private readonly http = inject(HttpClient);
  private readonly base = '/api/v1/colaboradores';

  listar(): Observable<Colaborador[]> {
    return this.http.get<Colaborador[]>(this.base);
  }

  criar(dto: { codigo: string; nome: string; codigoUnidade: string; codigoUsuario: string }): Observable<Colaborador> {
    return this.http.post<Colaborador>(this.base, dto);
  }

  // TODO: atualizar(id, dto) e remover(id)
}
