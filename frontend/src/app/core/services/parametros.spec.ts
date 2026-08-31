import { describe, expect, it } from 'vitest';
import { paginaDaUrl, paramsDePaginacao } from './parametros';

describe('paginaDaUrl', () => {
  it('aceita um número de página válido', () => {
    expect(paginaDaUrl('3')).toBe(3);
  });

  // A URL é digitável: tudo abaixo pode chegar aqui vindo do usuário, e nada disso
  // deve seguir para a API.
  it.each([
    ['ausente', null],
    ['vazio', ''],
    ['texto', 'abc'],
    ['zero', '0'],
    ['negativo', '-2'],
    ['decimal', '1.5']
  ])('cai na primeira página quando o valor é %s', (_caso, valor) => {
    expect(paginaDaUrl(valor)).toBe(1);
  });
});

describe('paramsDePaginacao', () => {
  it('não envia parâmetro algum quando nada é informado', () => {
    expect(paramsDePaginacao().keys()).toEqual([]);
  });

  it('omite o campo ausente em vez de mandar undefined', () => {
    const params = paramsDePaginacao({ pagina: 2 });

    expect(params.get('pagina')).toBe('2');
    expect(params.has('tamanho')).toBe(false);
  });

  it('envia os dois quando ambos vêm', () => {
    const params = paramsDePaginacao({ pagina: 3, tamanho: 50 });

    expect(params.get('pagina')).toBe('3');
    expect(params.get('tamanho')).toBe('50');
  });
});
