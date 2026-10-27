// Camada de acesso aos anúncios da plataforma.
//
// Isola as rotas da API num único lugar, seguindo o mesmo padrão do projeto
// (as telas não montam URL nem lidam com o wrapper fetch diretamente).
//
// Quando surgir o endpoint de Alimentação, basta somar aqui — a Home não precisa mudar.

import { api } from './api.js';

export async function listarItensBazar() {
  const dados = await api.get('/api/itens-bazar');

  if (!Array.isArray(dados)) {
    throw new Error('Resposta inesperada na listagem de anúncios.');
  }

  return dados;
}

export async function obterItemBazar(id) {
  const dados = await api.get(
    `/api/itens-bazar/${encodeURIComponent(id)}`,
  );

  if (!dados || typeof dados !== 'object' || !dados.id) {
    throw new Error('Resposta inesperada no detalhe do anúncio.');
  }

  return dados;
}