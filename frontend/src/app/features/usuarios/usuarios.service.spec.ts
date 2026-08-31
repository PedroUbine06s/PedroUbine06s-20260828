import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { Pagina, Usuario } from '../../core/models/modelos';
import { UsuariosService } from './usuarios.service';

describe('UsuariosService', () => {
  let servico: UsuariosService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    servico = TestBed.inject(UsuariosService);
    http = TestBed.inject(HttpTestingController);
  });

  // Garante que nenhuma requisição inesperada saiu.
  afterEach(() => http.verify());

  describe('listar', () => {
    it('não envia filtro algum quando nada é pedido', () => {
      servico.listar().subscribe();

      const req = http.expectOne(r => r.url === '/api/v1/usuarios');
      // Filtro ausente e filtro "false" são coisas diferentes: a API só filtra
      // quando o parâmetro vem, então ele não pode aparecer aqui.
      expect(req.request.params.has('ativo')).toBe(false);
      expect(req.request.params.has('semColaborador')).toBe(false);
      req.flush(paginaVazia());
    });

    it('envia ativo=false, e não o omite por ser falso', () => {
      servico.listar({ ativo: false }).subscribe();

      const req = http.expectOne(r => r.url === '/api/v1/usuarios');
      expect(req.request.params.get('ativo')).toBe('false');
      req.flush(paginaVazia());
    });

    // É a consulta que o formulário de colaborador faz: ativos e ainda sem vínculo.
    it('combina ativo com semColaborador', () => {
      servico.listar({ ativo: true, semColaborador: true, tamanho: 100 }).subscribe();

      const req = http.expectOne(r => r.url === '/api/v1/usuarios');
      expect(req.request.params.get('ativo')).toBe('true');
      expect(req.request.params.get('semColaborador')).toBe('true');
      expect(req.request.params.get('tamanho')).toBe('100');
      req.flush(paginaVazia());
    });

    it('devolve o envelope paginado como veio da API', () => {
      let recebido: Pagina<Usuario> | undefined;
      servico.listar({ pagina: 2 }).subscribe(p => (recebido = p));

      const pagina = { ...paginaVazia(), pagina: 2, total: 42, totalDePaginas: 3 };
      http.expectOne(r => r.url === '/api/v1/usuarios').flush(pagina);

      expect(recebido?.total).toBe(42);
      expect(recebido?.totalDePaginas).toBe(3);
    });
  });

  describe('atualizar', () => {
    it('envia apenas senha e status, espelhando o contrato restritivo do PUT', () => {
      servico.atualizar('abc-123', { senha: 'novasenha1', ativo: true }).subscribe();

      const req = http.expectOne('/api/v1/usuarios/abc-123');
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual({ senha: 'novasenha1', ativo: true });
      req.flush({});
    });

    it('omite a senha quando ela não foi informada, para não alterá-la', () => {
      servico.atualizar('abc-123', { senha: undefined, ativo: false }).subscribe();

      const req = http.expectOne('/api/v1/usuarios/abc-123');
      expect(req.request.body).toEqual({ senha: undefined, ativo: false });
      req.flush({});
    });
  });

  it('cria pelo POST sem informar código, que é gerado pelo sistema', () => {
    servico.criar({ login: 'novo.usuario', senha: 'senha12345', ativo: true }).subscribe();

    const req = http.expectOne('/api/v1/usuarios');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).not.toHaveProperty('codigo');
    req.flush({});
  });
});

function paginaVazia(): Pagina<Usuario> {
  return { itens: [], pagina: 1, tamanho: 20, total: 0, totalDePaginas: 0 };
}
