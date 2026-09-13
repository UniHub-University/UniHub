namespace UniHub.Application.DTOs.Vendas;

public record CreateItemPedidoDto(Guid ProdutoId, int Quantidade);

public record CreatePedidoDto(List<CreateItemPedidoDto> Itens);

public record PedidoResultDto(bool Sucesso, string Mensagem, Guid? PedidoId);