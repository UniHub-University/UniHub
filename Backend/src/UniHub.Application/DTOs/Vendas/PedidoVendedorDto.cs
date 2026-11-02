using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Vendas;

// P6: pedidos que contem produtos do vendedor autenticado.
// LGPD: do comprador so o NOME (sem e-mail, telefone, foto ou id).
// Itens e Total refletem APENAS os produtos deste vendedor, para ele nao ver
// o que outro vendedor vendeu no mesmo pedido.

public record PedidoVendedorItemDto(
    Guid ProdutoId,
    string ProdutoNome,
    int Quantidade,
    decimal PrecoUnitarioNaCompra);

public record PedidoVendedorDto(
    Guid PedidoId,
    PedidoStatus Status,
    TipoPedido Tipo,
    DateTime CreatedAt,
    DateTime? ExpiraEm,
    string CompradorNome,
    List<PedidoVendedorItemDto> Itens,
    decimal Total);