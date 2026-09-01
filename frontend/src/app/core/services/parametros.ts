import { HttpParams } from '@angular/common/http';
import { ParametrosPaginacao } from '../models/modelos';

/**
 * Monta a query string de paginação omitindo o que for indefinido, para não
 * enviar `?pagina=undefined` — o backend já aplica os próprios padrões.
 */
export function paramsDePaginacao(p: ParametrosPaginacao = {}): HttpParams {
  let params = new HttpParams();

  if (p.pagina !== undefined) params = params.set('pagina', p.pagina);
  if (p.tamanho !== undefined) params = params.set('tamanho', p.tamanho);

  return params;
}

/**
 * Lê o número de página vindo da URL.
 *
 * A URL é digitável, então qualquer coisa pode chegar aqui: texto, zero, negativo. Em vez
 * de propagar isso para a API, valores inválidos caem na primeira página.
 */
export function paginaDaUrl(valor: string | null): number {
  const numero = Number(valor);

  return Number.isInteger(numero) && numero > 0 ? numero : 1;
}

/** Tamanhos que o seletor do Carbon oferece. O teto da API é 100. */
const TAMANHOS_VALIDOS = [10, 20, 30, 40, 50];
export const TAMANHO_PADRAO = 20;

/**
 * Lê o tamanho de página vindo da URL. Valor fora da lista cai no padrão: a URL é digitável,
 * e um tamanho que o seletor não oferece deixaria o controle sem opção correspondente.
 */
export function tamanhoDaUrl(valor: string | null): number {
  const numero = Number(valor);

  return TAMANHOS_VALIDOS.includes(numero) ? numero : TAMANHO_PADRAO;
}
