using UniHub.Domain.Entities;

namespace UniHub.Application.Interfaces.Vendas;

public interface IProdutoRepository
{
    Task<Produto?> GetByIdAsync(Guid id);

    // Nucleo da solucao de concorrencia. Executa um update condicional atomico no banco:
    //    UPDATE "Produtos"
    //    SET "QuantidadeDisponivel" = "QuantidadeDisponivel" - @quantidade
    //    WHERE "Id" = @id AND "QuantidadeDisponivel" >= @quantidade

    // Retorna true somente se a linha foi realmente afetada. Isso garante atomicidade sem lock
    // de memoria, funcionando corretamente mesmo com múltiplas instâncias da API atras de um load balancer.

    Task<bool> TryDecrementarEstoqueAsync(Guid produtoId, int quantidade);

    // Usado pela compensacao (rollback) quando um pedido de varios itens falha 
    // no meio do processo -- devolve ao estoque a quantidade que ja tinha sido 
    // decrementada com sucesso.

    Task IncrementarEstoqueAsync(Guid produtoId, int quantidade);
}