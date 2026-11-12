// Camada de acesso ao módulo de Ação Solidária.
//
// Reúne TODAS as chamadas do módulo num único lugar (padrão do projeto: as telas
// não montam URL nem usam fetch direto). Organizado por seção para os dois lados
// do fluxo (solicitante e voluntário) consumirem sem conflito de arquivo.
//
// ⚠️ LGPD: nenhum endpoint aqui expõe a identidade do solicitante. Só o
// CodigoPublico pseudonimizado circula, até o solicitante autorizar a revelação
// via revelarIdentidade() (que ele mesmo dispara).

import { api } from './api.js';

// ---------------------------------------------------------------------------
// Solicitações de apoio (lado do SOLICITANTE) — SolicitacaoApoioController
// ---------------------------------------------------------------------------

// Cria uma solicitação. dto: { categoria: number (CategoriaApoio), descricao: string }.
export function criarSolicitacao({ categoria, descricao }) {
  return api.post('/api/SolicitacaoApoio', { categoria, descricao });
}

// Lista as solicitações ABERTAS (visão pública/anônima — só CodigoPublico).
export function listarSolicitacoesAbertas() {
  return api.get('/api/SolicitacaoApoio');
}

// Detalhe público de uma solicitação pelo id.
export function obterSolicitacao(id) {
  return api.get(`/api/SolicitacaoApoio/${encodeURIComponent(id)}`);
}

// Cancela a própria solicitação (só o autor autenticado; backend valida pelo token).
export function cancelarSolicitacao(id) {
  return api.patch(`/api/SolicitacaoApoio/${encodeURIComponent(id)}/cancelar`);
}

// NOTA: não existe (ainda) um endpoint "minhas solicitações" para o solicitante
// acompanhar o status das próprias. Gap de backend a confirmar com o squad/floater.
// Quando existir, adicionar aqui (ex.: listarMinhasSolicitacoes()).

// ---------------------------------------------------------------------------
// Perfil de voluntário (lado do VOLUNTÁRIO) — VoluntarioController
// ---------------------------------------------------------------------------

// Perfil de voluntário do usuário logado ({ codigoPublico, areasDeAtuacao, disponivel }).
export function obterMeuVoluntario() {
  return api.get('/api/perfil/voluntario');
}

// Torna o usuário logado um voluntário.
export function tornarSeVoluntario() {
  return api.post('/api/perfil/voluntario');
}

// Substitui a lista de áreas de atuação. areas: number[] (CategoriaApoio).
export function definirAreas(areas) {
  return api.put('/api/perfil/voluntario/areas', { areas });
}

// Pausa/retoma a atuação (deixa/volta a receber sugestões).
export function pausarAtuacao() {
  return api.patch('/api/perfil/voluntario/pausar');
}

export function retomarAtuacao() {
  return api.patch('/api/perfil/voluntario/retomar');
}

// ---------------------------------------------------------------------------
// Correspondências (o "match") — CorrespondenciaController
// ---------------------------------------------------------------------------

// Sugestões de solicitações compatíveis com as áreas do voluntário logado.
export function listarSugestoes() {
  return api.get('/api/correspondencias/sugestoes');
}

// Correspondências que o voluntário logado já aceitou (para acompanhar/concluir).
// Resposta anônima (só código público + categoria da solicitação — LGPD).
export function listarMinhasCorrespondencias() {
  return api.get('/api/correspondencias/minhas');
}

// Voluntário aceita uma solicitação. 409 se outra pessoa aceitou primeiro.
export function aceitarCorrespondencia(solicitacaoId) {
  return api.post('/api/correspondencias/aceitar', { solicitacaoId });
}

export function concluirCorrespondencia(id) {
  return api.post(`/api/correspondencias/${encodeURIComponent(id)}/concluir`);
}

export function cancelarCorrespondencia(id) {
  return api.post(`/api/correspondencias/${encodeURIComponent(id)}/cancelar`);
}

// Revela a identidade do solicitante. SÓ o solicitante pode chamar (LGPD).
export function revelarIdentidade(id) {
  return api.post(`/api/correspondencias/${encodeURIComponent(id)}/revelar-identidade`);
}

// ---------------------------------------------------------------------------
// Doações — DoacaoController
// ---------------------------------------------------------------------------

// Voluntário registra uma doação para uma solicitação que já aceitou.
// dto: { solicitacaoId: guid, descricao: string }.
export function criarDoacao({ solicitacaoId, descricao }) {
  return api.post('/api/doacoes', { solicitacaoId, descricao });
}

// Doações do voluntário logado.
export function listarMinhasDoacoes() {
  return api.get('/api/doacoes/minhas');
}

export function obterDoacao(id) {
  return api.get(`/api/doacoes/${encodeURIComponent(id)}`);
}

export function confirmarEntregaDoacao(id) {
  return api.post(`/api/doacoes/${encodeURIComponent(id)}/confirmar-entrega`);
}

export function cancelarDoacao(id) {
  return api.post(`/api/doacoes/${encodeURIComponent(id)}/cancelar`);
}