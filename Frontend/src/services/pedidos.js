// Camada de acesso aos pedidos (compra/reserva).
//
// Isola as rotas de /api/pedidos num único lugar, seguindo o padrão do projeto
// (as telas não montam URL nem lidam com o wrapper fetch diretamente).
//
// Contrato criação (P2/P3):
//   POST /api/pedidos  { tipo, itens: [{ produtoId, quantidade }] }
//   - tipo opcional: ausente => Compra. 0 = Compra, 1 = Reserva.
//   - Reserva só vale para ItemBazar (o backend recusa Alimento com 409).
//   - o comprador vem sempre do token (nunca enviamos id).
//
// Contrato vendedor (P6/P7):
//   GET  /api/pedidos/vendedor?page=&pageSize=  -> paginado (PedidoVendedorDto)
//   POST /api/pedidos/{id}/concluir             -> baixa manual (Reservado -> Concluido)
//   - o vendedor vem sempre do token; do comprador, só o nome (LGPD).

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

// P6: lista paginada dos pedidos que contêm produtos do vendedor autenticado.
// Resposta: { items, page, pageSize, totalItems, totalPages }, onde cada item é
// um PedidoVendedorDto { pedidoId, status, tipo, createdAt, expiraEm,
// compradorNome, itens[], total }.
export async function listarPedidosDoVendedor({ page = 1, pageSize = 10 } = {}) {
  return api.get(`/api/pedidos/vendedor?page=${page}&pageSize=${pageSize}`);
}

// P7: baixa manual de um pedido pelo vendedor (Reservado -> Concluido).
// Sem corpo. Lança ApiError em 403 (não é vendedor do pedido), 404 (inexistente)
// ou 409 (pedido não está mais Reservado).
export async function concluirPedido(pedidoId) {
  return api.post(`/api/pedidos/${pedidoId}/concluir`);
}