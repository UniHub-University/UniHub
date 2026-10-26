using UniHub.Domain.Entities;

namespace UniHub.Application.Interfaces.Vendas;

public interface IPedidoRepository
{
    Task<Pedido> AddAsync(Pedido pedido);
    Task<Pedido?> GetByIdAsync(Guid id);

    // Historico de pedidos: pedidos de UM comprador, do mais recente
    // para o mais antigo. page eh 1-based; o limite de pageSize eh validado no Service.
    // Devolve a pagina e o total de pedidos do comprador (para montar a resposta paginada).
    Task<(IReadOnlyList<Pedido> Pedidos, int Total)> GetByCompradorAsync(Guid compradorId, int page, int pageSize);
}