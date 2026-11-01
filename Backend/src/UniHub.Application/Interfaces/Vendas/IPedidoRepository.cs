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

    // P7: true se o pedido tem ao menos um item de um produto cujo Vendedor
    // pertence ao usuario informado (Produto.Vendedor.UsuarioId == usuarioId).
    Task<bool> PedidoTemItemDoVendedorAsync(Guid pedidoId, Guid usuarioId);

    // P7: baixa atomica. Atualiza Status para Concluido SOMENTE se o pedido ainda
    // estiver Reservado (UPDATE condicional). Devolve false se nenhuma linha foi
    // afetada (pedido inexistente ou que ja mudou de estado, ex.: expirou).
    Task<bool> TryConcluirAsync(Guid pedidoId);
}