using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Vendas;

// Feature: Historico-pedidos (BRV-89).
// DTOs de resposta do historico de pedidos do comprador autenticado.
// Nao expoem a entidade Usuario (Comprador): so dados do pedido e dos itens.
// O PrecoUnitarioNaCompra vem do ItemPedido, entao o historico nao muda
// retroativamente se o preco do Produto mudar depois.

public record HistoricoItemPedidoDto(
    Guid ProdutoId,
    string ProdutoNome,
    int Quantidade,
    decimal PrecoUnitarioNaCompra);

// Tipo (Compra/Reserva) e ExpiraEm (prazo da reserva; nulo na Compra) ficam no
// fim do record para nao quebrar quem ja consome os campos anteriores (P2/P3).
public record HistoricoPedidoDto(
    Guid PedidoId,
    PedidoStatus Status,
    DateTime CreatedAt,
    List<HistoricoItemPedidoDto> Itens,
    decimal Total,
    TipoPedido Tipo,
    DateTime? ExpiraEm);