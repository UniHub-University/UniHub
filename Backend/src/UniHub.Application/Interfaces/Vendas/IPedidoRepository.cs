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

    // P6: pedidos que contem ao menos um item de produto do vendedor (usuarioId e o
    // Usuario.Id vindo do token), do mais recente para o mais antigo. page eh 1-based.
    // Carrega Comprador, Itens, Produto e Vendedor para o Service filtrar so os
    // itens do vendedor. Devolve a pagina e o total (para a resposta paginada).
    Task<(IReadOnlyList<Pedido> Pedidos, int Total)> GetByVendedorAsync(Guid usuarioId, int page, int pageSize);

    // P4: reservas vencidas (Tipo = Reserva, Status = Reservado, ExpiraEm <= agora),
    // com os Itens carregados (ProdutoId + Quantidade) para o Service devolver o
    // estoque. Usado pelo BackgroundService de expiracao.
    Task<IReadOnlyList<Pedido>> GetReservasVencidasAsync(DateTime agora);

    // P4/P5: expiracao atomica. Define Status = Expirado SOMENTE se o pedido ainda
    // estiver Reservado (UPDATE condicional). Devolve false se nenhuma linha foi
    // afetada (ja concluido/cancelado/expirado por outro caminho concorrente),
    // garantindo que o estoque seja devolvido uma unica vez.
    Task<bool> TryExpirarAsync(Guid pedidoId);
}