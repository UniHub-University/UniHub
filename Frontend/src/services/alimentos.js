// Camada de acesso à API do módulo de Alimentação.
//
// As páginas não fazem fetch diretamente.
// Todas as requisições passam pelo wrapper api.js,
// que injeta o token JWT e trata os erros HTTP.

import { api } from './api.js';

export async function listarAlimentos() {
  const dados = await api.get('/api/alimentos');

  if (!Array.isArray(dados)) {
    throw new Error('Resposta inesperada na listagem de alimentos.');
  }

  return dados;
}

export async function obterAlimento(id) {
  const dados = await api.get(
    `/api/alimentos/${encodeURIComponent(id)}`,
  );

  if (!dados || typeof dados !== 'object' || !dados.id) {
    throw new Error('Resposta inesperada no detalhe do alimento.');
  }

  return dados;
}

export async function criarAlimento(dados) {
  return api.post('/api/alimentos', dados);
}

export async function atualizarAlimento(id, dados) {
  return api.put(
    `/api/alimentos/${encodeURIComponent(id)}`,
    dados,
  );
}

export async function inativarAlimento(id) {
  return api.delete(
    `/api/alimentos/${encodeURIComponent(id)}`,
  );
}

export async function reativarAlimento(id) {
  return api.patch(
    `/api/alimentos/${encodeURIComponent(id)}/reativar`,
  );
}