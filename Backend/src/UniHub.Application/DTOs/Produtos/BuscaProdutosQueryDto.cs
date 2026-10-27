using UniHub.Application.DTOs;

namespace UniHub.Application.DTOs.Produtos;

// Parametros de query string de GET /api/produtos. Tudo opcional, exceto
// paginacao, que tem valores padrao. A validacao/normalizacao fica no Service.
public class BuscaProdutosQueryDto
{
    public string? Termo { get; set; }       // busca em Nome e Descricao
    public string? Tipo { get; set; }         // "bazar" | "lanche"
    public decimal? PrecoMin { get; set; }
    public decimal? PrecoMax { get; set; }
    public string? Campus { get; set; }
    public int? DiaSemana { get; set; }       // 0=Domingo ... 6=Sabado
    public string? Hora { get; set; }         // "HH:mm"
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

public enum TipoProduto
{
    Bazar = 0,
    Lanche = 1
}

// Filtros ja validados e convertidos pelo Service, entregues ao repositorio.
public record FiltrosBuscaProdutos(
    string? Termo,
    TipoProduto? Tipo,
    decimal? PrecoMin,
    decimal? PrecoMax,
    string? Campus,
    DiaSemana? DiaSemana,
    TimeSpan? Hora,
    int Page,
    int PageSize
);
