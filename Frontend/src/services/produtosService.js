// Serviço de busca de produtos (BRV-6). Fica sobre o wrapper api.js, que não
// monta query string — por isso montamos a URLSearchParams aqui, ignorando
// filtros vazios para não poluir a URL nem o backend.

import { api } from './api.js';

export function buscarProdutos(filtros = {}) {
  const params = new URLSearchParams();

  Object.entries(filtros).forEach(([chave, valor]) => {
    if (valor !== undefined && valor !== null && String(valor).trim() !== '') {
      params.append(chave, String(valor).trim());
    }
  });

  const queryString = params.toString();
  return api.get(`/api/produtos${queryString ? `?${queryString}` : ''}`);
}
