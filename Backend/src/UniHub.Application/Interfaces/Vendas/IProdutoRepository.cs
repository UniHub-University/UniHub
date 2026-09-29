using UniHub.Domain.Entities;

namespace UniHub.Application.Interfaces.Vendas;

public interface IProdutoRepository
{
    Task<Produto?> GetByIdAsync(Guid id);

    Task<bool> TryDecrementarEstoqueAsync(Guid produtoId, int quantidade);

    Task IncrementarEstoqueAsync(Guid produtoId, int quantidade);
}