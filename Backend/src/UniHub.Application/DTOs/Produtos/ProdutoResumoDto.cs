namespace UniHub.Application.DTOs.Produtos;

// Projecao enxuta de um produto para a listagem/busca. Expoe apenas dados
// publicos do anuncio e o nome do vendedor.
public record ProdutoResumoDto(
    Guid Id,
    string Nome,
    string? Descricao,
    decimal Preco,
    int QuantidadeDisponivel,
    string Tipo,                 // "bazar" | "lanche"
    DateTime DataPublicacao,
    Guid VendedorId,
    string NomeVendedor,
    List<string> Campi
);
