import { api, ApiError } from './api.js';

const rota = '/api/itens-bazar';
export const obterItemBazarParaEdicao = (id) => api.get(`${rota}/${encodeURIComponent(id)}/edicao`);
export const criarItemBazar = (dados) => api.post(rota, dados);
export const atualizarItemBazar = (id, dados) => api.put(`${rota}/${encodeURIComponent(id)}`, dados);
export const excluirItemBazar = (id) => api.delete(`${rota}/${encodeURIComponent(id)}`);

export function mensagemErroItem(erro) {
  if (erro instanceof ApiError) {
    if (erro.status === 401) return 'Sua sessão expirou. Entre novamente para continuar.';
    if (erro.status === 403) return 'Você não tem autorização para editar/excluir esse produto.';
    if (erro.status === 404) return 'Este item não foi encontrado. Ele pode ter sido excluído.';
    if (erro.status >= 500) return 'O servidor não conseguiu concluir a operação. Tente novamente mais tarde.';
    return erro.message;
  }
  return 'Não foi possível concluir a operação. Tente novamente.';
}

// A API pode serializar enums por número ou por nome. Não use Number(nome).
export function normalizarEnumItem(valor, tipo) {
  const nomes = tipo === 'condicao'
    ? { novo: 0, seminovo: 1, seminnovo: 1, usado: 2 }
    : { disponivel: 0, esgotado: 1, inativo: 2 };
  if (typeof valor === 'number') return [0, 1, 2].includes(valor) ? valor : '';
  if (typeof valor !== 'string' || !valor.trim()) return '';
  const chave = valor.trim().normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
  if (/^[012]$/.test(chave)) return Number(chave);
  return nomes[chave] ?? '';
}