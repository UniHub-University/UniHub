// Camada de acesso aos pedidos (compra/reserva).
//
// Isola a rota POST /api/pedidos num único lugar, seguindo o padrão do projeto
// (as telas não montam URL nem lidam com o wrapper fetch diretamente).
//
// Contrato (P2/P3 da Frente B):
//   POST /api/pedidos  { tipo, itens: [{ produtoId, quantidade }] }
//   - tipo é opcional: ausente => Compra. 0 = Compra, 1 = Reserva.
//   - Reserva só vale para ItemBazar (o backend recusa Alimento com 409).
//   - 200 => { sucesso, mensagem, pedidoId }
//   - 409 => { sucesso: false, mensagem: "<motivo>", pedidoId: null }
//   - o comprador vem sempre do token (nunca enviamos id).

import { api } from './api.js';

// Espelha o enum TipoPedido do backend.
export const TIPO_PEDIDO = {
  COMPRA: 0,
  RESERVA: 1,
};

// Cria um pedido de UM produto na quantidade informada.
// `tipo` default Compra. Lança ApiError (tratado pela tela) em caso de 409/401.
export async function criarPedido({ produtoId, quantidade, tipo = TIPO_PEDIDO.COMPRA }) {
  return api.post('/api/pedidos', {
    tipo,
    itens: [{ produtoId, quantidade }],
  });
}