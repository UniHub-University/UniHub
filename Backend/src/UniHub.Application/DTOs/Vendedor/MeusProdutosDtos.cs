using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs;

// Produto do próprio vendedor. Inclui Status porque a listagem mostra também
// os esgotados e os inativados, que a busca pública esconde.
public record MeuProdutoDto(
    Guid Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    int QuantidadeDisponivel,
    string Tipo,              // "bazar" | "lanche"
    ProdutoStatus Status,
    DateTime DataPublicacao
);

public record MeusProdutosResultDto(bool Sucesso, string Mensagem, IReadOnlyList<MeuProdutoDto> Produtos);
