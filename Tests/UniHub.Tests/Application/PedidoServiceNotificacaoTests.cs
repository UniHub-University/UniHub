using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using UniHub.Application.DTOs.Vendas;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Application.Services.Vendas;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Application;

// A4 (BRV-198): emissao dos eventos do painel em tempo real ao criar pedido.
// Foca o CONTRATO de notificacao (quem recebe e com que payload), nao a regra
// de estoque (coberta em PedidoServiceTests).
[Trait("Categoria", "Unit")]
public class PedidoServiceNotificacaoTests
{
    private readonly IProdutoRepository _produtoRepo = Substitute.For<IProdutoRepository>();
    private readonly IPedidoRepository _pedidoRepo = Substitute.For<IPedidoRepository>();
    private readonly INotificadorPainel _notificadorPainel = Substitute.For<INotificadorPainel>();

    private PedidoService CriarService() => new(_produtoRepo, _pedidoRepo, _notificadorPainel);

    private static ItemBazar NovoProduto(Guid vendedorId, int quantidade, decimal preco = 10m) => new()
    {
        Id = Guid.NewGuid(),
        Nome = "Produto Teste",
        Preco = preco,
        Status = ProdutoStatus.Disponivel,
        QuantidadeDisponivel = quantidade,
        VendedorId = vendedorId
    };

    // ---------- EstoqueAtualizado: vendedor dono + quantidade resultante ----------
    [Fact]
    public async Task CreatePedido_QuandoSucesso_NotificaEstoqueDoVendedorComQuantidadeResultante()
    {
        var service = CriarService();
        var vendedorId = Guid.NewGuid();
        var produto = NovoProduto(vendedorId, quantidade: 10);
        _produtoRepo.GetByIdAsync(produto.Id).Returns(produto);
        _produtoRepo.TryDecrementarEstoqueAsync(produto.Id, 3).Returns(true);
        _pedidoRepo.AddAsync(Arg.Any<Pedido>()).Returns(call => call.Arg<Pedido>());

        var request = new CreatePedidoDto(new List<CreateItemPedidoDto> { new(produto.Id, 3) });

        await service.CreatePedidoAsync(Guid.NewGuid(), request);

        // quantidade resultante = 10 - 3 = 7, status segue Disponivel
        await _notificadorPainel.Received(1).NotificarEstoqueAsync(
            vendedorId,
            Arg.Is<EstoqueAtualizadoDto>(e =>
                e.ProdutoId == produto.Id &&
                e.QuantidadeDisponivel == 7 &&
                e.Status == ProdutoStatus.Disponivel));
    }

    // ---------- EstoqueAtualizado: ao zerar, status derivado Esgotado ----------
    [Fact]
    public async Task CreatePedido_QuandoEstoqueZera_NotificaStatusEsgotado()
    {
        var service = CriarService();
        var vendedorId = Guid.NewGuid();
        var produto = NovoProduto(vendedorId, quantidade: 2);
        _produtoRepo.GetByIdAsync(produto.Id).Returns(produto);
        _produtoRepo.TryDecrementarEstoqueAsync(produto.Id, 2).Returns(true);
        _pedidoRepo.AddAsync(Arg.Any<Pedido>()).Returns(call => call.Arg<Pedido>());

        var request = new CreatePedidoDto(new List<CreateItemPedidoDto> { new(produto.Id, 2) });

        await service.CreatePedidoAsync(Guid.NewGuid(), request);

        await _notificadorPainel.Received(1).NotificarEstoqueAsync(
            vendedorId,
            Arg.Is<EstoqueAtualizadoDto>(e =>
                e.QuantidadeDisponivel == 0 &&
                e.Status == ProdutoStatus.Esgotado));
    }

    // ---------- NovoPedido: payload com dados do produto ----------
    [Fact]
    public async Task CreatePedido_QuandoSucesso_NotificaNovoPedidoDoVendedor()
    {
        var service = CriarService();
        var vendedorId = Guid.NewGuid();
        var produto = NovoProduto(vendedorId, quantidade: 5);
        _produtoRepo.GetByIdAsync(produto.Id).Returns(produto);
        _produtoRepo.TryDecrementarEstoqueAsync(produto.Id, 1).Returns(true);
        _pedidoRepo.AddAsync(Arg.Any<Pedido>()).Returns(call => call.Arg<Pedido>());

        var request = new CreatePedidoDto(new List<CreateItemPedidoDto> { new(produto.Id, 1) });

        await service.CreatePedidoAsync(Guid.NewGuid(), request);

        await _notificadorPainel.Received(1).NotificarNovoPedidoAsync(
            vendedorId,
            Arg.Is<NovoPedidoDto>(e =>
                e.ProdutoId == produto.Id &&
                e.NomeProduto == produto.Nome &&
                e.Quantidade == 1));
    }

    // ---------- Falha de estoque: nao notifica nada ----------
    [Fact]
    public async Task CreatePedido_QuandoEstoqueInsuficiente_NaoNotifica()
    {
        var service = CriarService();
        var produto = NovoProduto(Guid.NewGuid(), quantidade: 1);
        _produtoRepo.GetByIdAsync(produto.Id).Returns(produto);
        _produtoRepo.TryDecrementarEstoqueAsync(produto.Id, 5).Returns(false);

        var request = new CreatePedidoDto(new List<CreateItemPedidoDto> { new(produto.Id, 5) });

        await service.CreatePedidoAsync(Guid.NewGuid(), request);

        await _notificadorPainel.DidNotReceive()
            .NotificarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<EstoqueAtualizadoDto>());
        await _notificadorPainel.DidNotReceive()
            .NotificarNovoPedidoAsync(Arg.Any<Guid>(), Arg.Any<NovoPedidoDto>());
    }

    // ---------- Falha do hub nao derruba a venda (degradacao graciosa) ----------
    [Fact]
    public async Task CreatePedido_QuandoHubFalha_PedidoAindaEhCriadoComSucesso()
    {
        var service = CriarService();
        var produto = NovoProduto(Guid.NewGuid(), quantidade: 5);
        _produtoRepo.GetByIdAsync(produto.Id).Returns(produto);
        _produtoRepo.TryDecrementarEstoqueAsync(produto.Id, 1).Returns(true);
        _pedidoRepo.AddAsync(Arg.Any<Pedido>()).Returns(call => call.Arg<Pedido>());

        // o hub lanca ao notificar -> o service deve engolir (best-effort)
        _notificadorPainel
            .NotificarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<EstoqueAtualizadoDto>())
            .Returns<Task>(_ => throw new Exception("hub fora do ar"));

        var request = new CreatePedidoDto(new List<CreateItemPedidoDto> { new(produto.Id, 1) });

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.PedidoId);
    }
}
