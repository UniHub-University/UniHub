using UniHub.Domain.Entities;

namespace UniHub.Application.Interfaces.Vendas;

public interface IProdutoRepository
{
    Task<Produto?> GetByIdAsync(Guid id);

    // Indica se o produto pertence ao usuario informado (dono = Vendedor.UsuarioId).
    // Usado para impedir que alguem compre/reserve o proprio produto.
    Task<bool> ProdutoPertenceAoUsuarioAsync(Guid produtoId, Guid usuarioId);

    Task<bool> TryDecrementarEstoqueAsync(Guid produtoId, int quantidade);

    Task IncrementarEstoqueAsync(Guid produtoId, int quantidade);

     // Exclusão lógica: marca o produto como Inativo em vez de apagar a
    // linha do banco. Preserva o histórico de ItensPedido que referenciam
    // esse produto. Retorna false se o produto NAO existir.
    Task <bool> InativarAsync(Guid produtoId);
}