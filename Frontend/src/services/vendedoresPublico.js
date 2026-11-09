// Servico da visao PUBLICA de vendedores (perfil, lista e anuncios de um
// vendedor). Fica sobre o wrapper api.js, que nao monta query string; por isso
// a URLSearchParams e montada aqui.

import { api } from './api.js';

function comPaginacao(caminho, { page, pageSize } = {}) {
  const params = new URLSearchParams();
  if (page) params.set('page', String(page));
  if (pageSize) params.set('pageSize', String(pageSize));
  const queryString = params.toString();
  return queryString ? `${caminho}?${queryString}` : caminho;
}

// GET /api/vendedores/{id}: perfil publico (nome, foto, descricao, nota, locais
// e horarios). Lanca ApiError com status 404 se o vendedor nao existe.
export function obterVendedorPublico(id) {
  return api.get(`/api/vendedores/${encodeURIComponent(id)}`);
}

// GET /api/vendedores?page=&pageSize=: lista paginada de vendedores que tem
// anuncio disponivel, dos mais bem avaliados para os menos.
export function listarVendedores(paginacao) {
  return api.get(comPaginacao('/api/vendedores', paginacao));
}

// GET /api/vendedores/{id}/produtos?page=&pageSize=: anuncios disponiveis de um
// vendedor, no mesmo formato de GET /api/produtos.
export function listarProdutosDoVendedor(id, paginacao) {
  return api.get(comPaginacao(`/api/vendedores/${encodeURIComponent(id)}/produtos`, paginacao));
}
