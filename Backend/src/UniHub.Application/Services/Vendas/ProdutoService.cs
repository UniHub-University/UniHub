using System.Globalization;
using UniHub.Application.DTOs;
using UniHub.Application.DTOs.Common;
using UniHub.Application.DTOs.Produtos;
using UniHub.Application.Interfaces.Vendas;

namespace UniHub.Application.Services.Vendas;

public class ProdutoService
{
    private const int TamanhoMaximoPagina = 50;

    private readonly IProdutoRepository _produtoRepository;
    private readonly IProdutoBuscaRepository _produtoBuscaRepository;

    public ProdutoService(IProdutoRepository produtoRepository, IProdutoBuscaRepository produtoBuscaRepository)
    {
        _produtoRepository = produtoRepository;
        _produtoBuscaRepository = produtoBuscaRepository;
    }

    // Lugar para qualquer regra de negócio futura
    public async Task<bool> InativarProdutoAsync(Guid produtoId)
    {
        return await _produtoRepository.InativarAsync(produtoId);
    }

    // Valida e normaliza os filtros vindos da API antes de consultar o
    // repositorio. Lanca ArgumentException em entrada invalida (o controller
    // converte para 400), evitando 500 por enum/formato invalido.
    public async Task<PaginatedResultDto<ProdutoResumoDto>> BuscarAsync(BuscaProdutosQueryDto query)
    {
        TipoProduto? tipo = null;
        if (!string.IsNullOrWhiteSpace(query.Tipo))
        {
            if (!Enum.TryParse<TipoProduto>(query.Tipo, ignoreCase: true, out var tipoConvertido)
                || !Enum.IsDefined(tipoConvertido))
                throw new ArgumentException("Tipo invalido. Use 'bazar' ou 'lanche'.");
            tipo = tipoConvertido;
        }

        if (query.PrecoMin is < 0 || query.PrecoMax is < 0)
            throw new ArgumentException("Preco nao pode ser negativo.");
        if (query.PrecoMin.HasValue && query.PrecoMax.HasValue && query.PrecoMin > query.PrecoMax)
            throw new ArgumentException("Preco minimo nao pode ser maior que o maximo.");

        DiaSemana? dia = null;
        if (query.DiaSemana.HasValue)
        {
            if (!Enum.IsDefined(typeof(DiaSemana), query.DiaSemana.Value))
                throw new ArgumentException("Dia da semana invalido (0 a 6).");
            dia = (DiaSemana)query.DiaSemana.Value;
        }

        TimeSpan? hora = null;
        if (!string.IsNullOrWhiteSpace(query.Hora))
        {
            if (!TimeSpan.TryParseExact(query.Hora, @"hh\:mm", CultureInfo.InvariantCulture, out var horaConvertida))
                throw new ArgumentException("Hora invalida. Use o formato HH:mm.");
            hora = horaConvertida;
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, TamanhoMaximoPagina);

        var filtros = new FiltrosBuscaProdutos(
            query.Termo?.Trim(),
            tipo,
            query.PrecoMin,
            query.PrecoMax,
            query.Campus?.Trim(),
            dia,
            hora,
            page,
            pageSize);

        return await _produtoBuscaRepository.BuscarAsync(filtros);
    }
}
