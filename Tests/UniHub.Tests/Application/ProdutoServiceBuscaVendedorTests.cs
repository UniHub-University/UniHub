using NSubstitute;
using UniHub.Application.DTOs.Common;
using UniHub.Application.DTOs.Produtos;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Application.Services.Vendas;
using Xunit;

namespace UniHub.Tests.Application;

// Testes do filtro por vendedor na busca de produtos (ProdutoService.BuscarAsync).
[Trait("Categoria", "Unit")]
public class ProdutoServiceBuscaVendedorTests
{
    private readonly IProdutoRepository _produtoRepo = Substitute.For<IProdutoRepository>();
    private readonly IProdutoBuscaRepository _buscaRepo = Substitute.For<IProdutoBuscaRepository>();

    private ProdutoService CriarService()
    {
        _buscaRepo.BuscarAsync(Arg.Any<FiltrosBuscaProdutos>())
            .Returns(new PaginatedResultDto<ProdutoResumoDto>(new List<ProdutoResumoDto>(), 1, 12, 0, 0));
        return new ProdutoService(_produtoRepo, _buscaRepo);
    }

    [Fact]
    public async Task Buscar_ComVendedorId_RepassaOFiltroAoRepositorio()
    {
        var vendedorId = Guid.NewGuid();

        await CriarService().BuscarAsync(new BuscaProdutosQueryDto(), vendedorId);

        await _buscaRepo.Received(1).BuscarAsync(
            Arg.Is<FiltrosBuscaProdutos>(f => f.VendedorId == vendedorId));
    }

    [Fact]
    public async Task Buscar_SemVendedorId_NaoFiltraPorVendedor()
    {
        await CriarService().BuscarAsync(new BuscaProdutosQueryDto());

        await _buscaRepo.Received(1).BuscarAsync(
            Arg.Is<FiltrosBuscaProdutos>(f => f.VendedorId == null));
    }

    [Fact]
    public async Task Buscar_ComVendedorId_MantemAsDemaisValidacoes()
    {
        var query = new BuscaProdutosQueryDto { PrecoMin = 10, PrecoMax = 5 };

        await Assert.ThrowsAsync<ArgumentException>(
            () => CriarService().BuscarAsync(query, Guid.NewGuid()));
    }

    [Fact]
    public async Task Buscar_ComVendedorId_MantemOsFiltrosCombinados()
    {
        var vendedorId = Guid.NewGuid();
        var query = new BuscaProdutosQueryDto { Termo = " bolo ", Tipo = "lanche", Page = 2, PageSize = 500 };

        await CriarService().BuscarAsync(query, vendedorId);

        await _buscaRepo.Received(1).BuscarAsync(Arg.Is<FiltrosBuscaProdutos>(f =>
            f.VendedorId == vendedorId
            && f.Termo == "bolo"
            && f.Tipo == TipoProduto.Lanche
            && f.Page == 2
            && f.PageSize == 50));
    }
}
