using UniHub.Domain.Entities;

namespace UniHub.Application.Interfaces.Vendas;

public interface IPedidoRepository
{
    Task<Pedido> AddAsync(Pedido pedido);
    Task<Pedido?> GetByIdAsync(Guid id);
}