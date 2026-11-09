// Serviço de tempo real do Painel do Vendedor (Frente 2 / SignalR).
//
// Encapsula a conexão SignalR num único lugar, no espírito do AGENTS §14
// (acesso à API centralizado): nenhuma tela monta HubConnection na mão.
// Reusa a URL base de config/env.js e o JWT de authStorage.js — o mesmo token
// usado pelo wrapper REST (services/api.js).
//
// Contrato de eventos (fechado com o backend, Fase 0 do plano). Payloads em
// camelCase; "status" chega como ENUM NUMÉRICO (ProdutoStatus), igual ao estado
// inicial (MeuProdutoDto.Status) — o projeto não serializa enum como string:
//   - "EstoqueAtualizado": { produtoId, quantidadeDisponivel, status }  // status: number
//   - "NovoPedido":        { pedidoId, produtoId, nomeProduto, quantidade, quandoUtc }
// Hub em: {API_BASE_URL}/hubs/painel

import {
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from '@microsoft/signalr';
import { API_BASE_URL } from '../config/env.js';
import { obterToken } from '../auth/authStorage.js';

const URL_HUB = `${API_BASE_URL}/hubs/painel`;

// Nomes dos eventos — centralizados para o handler e o backend não divergirem.
export const EVENTOS = {
  ESTOQUE_ATUALIZADO: 'EstoqueAtualizado',
  NOVO_PEDIDO: 'NovoPedido',
};

let conexao = null;

/**
 * Abre (ou reusa) a conexão com o PainelHub e registra os handlers de evento.
 *
 * @param {object} handlers
 * @param {(payload: {produtoId: string, quantidadeDisponivel: number, status: number}) => void} [handlers.onEstoqueAtualizado]
 * @param {(payload: {pedidoId: string, produtoId: string, nomeProduto: string, quantidade: number, quandoUtc: string}) => void} [handlers.onNovoPedido]
 * @param {(estado: 'conectado' | 'reconectando' | 'desconectado') => void} [handlers.onMudancaConexao]
 * @returns {Promise<void>}
 */
export async function conectar(handlers = {}) {
  // Já conectado/conectando: não abre uma segunda conexão.
  if (conexao && conexao.state !== HubConnectionState.Disconnected) {
    return;
  }

  conexao = new HubConnectionBuilder()
    .withUrl(URL_HUB, {
      // WebSocket não envia header Authorization; o token vai pela query
      // string (?access_token=...), lido pelo backend em OnMessageReceived.
      accessTokenFactory: () => obterToken() ?? '',
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();

  // Eventos de negócio (registrados só se o chamador passou o handler).
  if (handlers.onEstoqueAtualizado) {
    conexao.on(EVENTOS.ESTOQUE_ATUALIZADO, handlers.onEstoqueAtualizado);
  }
  if (handlers.onNovoPedido) {
    conexao.on(EVENTOS.NOVO_PEDIDO, handlers.onNovoPedido);
  }

  // Ciclo de vida da conexão -> alimenta o indicador "ao vivo"/"reconectando".
  if (handlers.onMudancaConexao) {
    conexao.onreconnecting(() => handlers.onMudancaConexao('reconectando'));
    conexao.onreconnected(() => handlers.onMudancaConexao('conectado'));
    conexao.onclose(() => handlers.onMudancaConexao('desconectado'));
  }

  await conexao.start();
  handlers.onMudancaConexao?.('conectado');
}

/**
 * Encerra a conexão e limpa o estado. Seguro chamar mesmo sem conexão ativa
 * (ex.: no cleanup de um useEffect que nunca chegou a conectar).
 */
export async function desconectar() {
  if (!conexao) return;

  const atual = conexao;
  conexao = null;
  try {
    await atual.stop();
  } catch {
    // Encerramento é best-effort: se já caiu, não há o que fazer.
  }
}
