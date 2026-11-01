using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Vendas;

public record CreateItemPedidoDto(Guid ProdutoId, int Quantidade);

// Tipo e opcional: se o cliente nao enviar, o pedido e uma Compra (comportamento
// anterior). Reserva e temporaria e so vale para ItemBazar (validado no Service).
public record CreatePedidoDto(List<CreateItemPedidoDto> Itens, TipoPedido Tipo = TipoPedido.Compra);

public record PedidoResultDto(bool Sucesso, string Mensagem, Guid? PedidoId);