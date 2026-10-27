using System;
using System.Threading.Tasks;
using NSubstitute;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Application.Services.Vendas;
using Xunit;

namespace UniHub.Tests.Application;

/// <summary>
/// Testes de unidade do ProdutoService.
///
/// O service depende apenas de IProdutoRepository (interface), então usamos
/// um substituto NSubstitute — sem banco.
///
/// Hoje InativarProdutoAsync é um passa-direto: delega ao repositório e
/// devolve o bool. Os testes garantem que o service repassa a chamada com o
/// id correto e propaga o resultado (true/false). Se amanhã entrar regra de
/// negócio no service, estes testes já cercam o comportamento atual.
/// </summary>
[Trait("Categoria", "Unit")]
public class ProdutoServiceTests
{
    private readonly IProdutoRepository _produtoRepo = Substitute.For<IProdutoRepository>();
    private readonly IProdutoBuscaRepository _produtoBuscaRepo = Substitute.For<IProdutoBuscaRepository>(); 

    

    private ProdutoService CriarService() => new(_produtoRepo, _produtoBuscaRepo);

    [Fact]
    public async Task InativarProduto_QuandoRepositorioInativa_RetornaTrue()
    {
        var service = CriarService();
        var produtoId = Guid.NewGuid();
        _produtoRepo.InativarAsync(produtoId).Returns(true);

        var resultado = await service.InativarProdutoAsync(produtoId);

        Assert.True(resultado);
        await _produtoRepo.Received(1).InativarAsync(produtoId);
    }

    [Fact]
    public async Task InativarProduto_QuandoProdutoNaoExiste_RetornaFalse()
    {
        var service = CriarService();
        var produtoId = Guid.NewGuid();
        // repositorio devolve false quando o produto nao existe
        _produtoRepo.InativarAsync(produtoId).Returns(false);

        var resultado = await service.InativarProdutoAsync(produtoId);

        Assert.False(resultado);
        await _produtoRepo.Received(1).InativarAsync(produtoId);
    }
}
