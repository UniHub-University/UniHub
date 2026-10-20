// Persistência do JWT emitido pela API UniHub.
//
// Decisão: usamos localStorage por simplicidade nesta fase (Sprint 4), o que
// mantém o usuário logado entre recarregamentos. É o suficiente para tornar o
// produto demonstrável. Caso o squad decida endurecer a segurança depois
// (ex.: cookie httpOnly), basta trocar a implementação aqui — o resto do app
// consome apenas estas funções.

const CHAVE_TOKEN = 'unihub.token';
const CHAVE_USUARIO = 'unihub.usuario';

export function salvarToken(token) {
  if (token) {
    localStorage.setItem(CHAVE_TOKEN, token);
  }
}

export function obterToken() {
  return localStorage.getItem(CHAVE_TOKEN);
}

export function removerToken() {
  localStorage.removeItem(CHAVE_TOKEN);
  localStorage.removeItem(CHAVE_USUARIO);
}

export function salvarUsuario(usuario) {
  if (usuario) {
    localStorage.setItem(CHAVE_USUARIO, JSON.stringify(usuario));
  }
}

export function obterUsuario() {
  const bruto = localStorage.getItem(CHAVE_USUARIO);
  if (!bruto) return null;
  try {
    return JSON.parse(bruto);
  } catch {
    return null;
  }
}

export function estaAutenticado() {
  return Boolean(obterToken());
}