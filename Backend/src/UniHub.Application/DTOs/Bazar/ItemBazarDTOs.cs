using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Bazar;

public record CriarItemBazarDto(
    string Nome,
    string? Descricao,
    decimal Preco,
    int QuantidadeDisponivel,
    string Categoria,
    CondicaoItem Condicao
);

public record AtualizarItemBazarDto(
    string Nome,
    string? Descricao,
    decimal Preco,
    int QuantidadeDisponivel,
    string Categoria,
    CondicaoItem Condicao,
    ProdutoStatus Status
);

public record ItemBazarRespostaDto(
    Guid Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    int QuantidadeDisponivel,
    string Categoria,
    CondicaoItem Condicao,
    ProdutoStatus Status,
    DateTime DataPublicacao,
    Guid VendedorId
);