using System;
using System.Threading.Tasks;
using NSubstitute;
using UniHub.Application.DTOs.Vendas;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Application.Services.Vendas;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Application;

// Testes do cancelamento de reserva pelo comprador (PedidoService.CancelarReservaAsync).
[Trait("Categoria", "Unit")]
public class PedidoServiceCancelarTests
{
    private readonly IProdutoRepository _produtoRepo = Substitute.For<IProdutoRepository>();
    private readonly IPedidoRepository _pedidoRepo = Substitute.For<IPedidoRepository>();
    private readonly INotificadorPainel _notificadorPainel = Substitute.For<INotificadorPainel>();

    private PedidoService CriarService() => new(_produtoRepo, _pedidoRepo, _notificadorPainel);

    private static readonly Guid ProdutoA = Guid.NewGuid();
    private static readonly Guid ProdutoB = Guid.NewGuid();

    // Reserva com dois itens (A x2 e B x1), por padrao ainda Reservada.
    private static Pedido NovaReserva(
        Guid compradorId,
        PedidoStatus status = PedidoStatus.Reservado,
        TipoPedido tipo = TipoPedido.Reserva)
    {
        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            CompradorId = compradorId,
            Tipo = tipo,
            Status = status
        };
        pedido.Itens.Add(new ItemPedido { ProdutoId = ProdutoA, Quantidade = 2, PrecoUnitarioNaCompra = 10m });
        pedido.Itens.Add(new ItemPedido { ProdutoId = ProdutoB, Quantidade = 1, PrecoUnitarioNaCompra = 5m });
        return pedido;
    }

    // ---------- 404 ----------
    [Fact]
    public async Task Cancelar_QuandoPedidoNaoExiste_RetornaNaoEncontradoSemGravar()
    {
        var service = CriarService();
        var pedidoId = Guid.NewGuid();
        _pedidoRepo.GetByIdAsync(pedidoId).Returns((Pedido?)null);

        var resultado = await service.CancelarReservaAsync(pedidoId, Guid.NewGuid());

        Assert.Equal(CancelarReservaStatus.NaoEncontrado, resultado.Status);
        await _pedidoRepo.DidNotReceive().TryCancelarAsync(Arg.Any<Guid>());
        await _produtoRepo.DidNotReceive().IncrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }

    // ---------- 403: pedido de outro comprador ----------
    [Fact]
    public async Task Cancelar_QuandoPedidoEDeOutroComprador_RetornaProibidoSemGravar()
    {
        var service = CriarService();
        var pedido = NovaReserva(compradorId: Guid.NewGuid());
        _pedidoRepo.GetByIdAsync(pedido.Id).Returns(pedido);

        var resultado = await service.CancelarReservaAsync(pedido.Id, Guid.NewGuid());

        Assert.Equal(CancelarReservaStatus.Proibido, resultado.Status);
        await _pedidoRepo.DidNotReceive().TryCancelarAsync(Arg.Any<Guid>());
        await _produtoRepo.DidNotReceive().IncrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }

    // ---------- 409: compra nao se cancela ----------
    [Fact]
    public async Task Cancelar_QuandoEUmaCompra_RetornaConflitoSemGravarNemDevolverEstoque()
    {
        var service = CriarService();
        var compradorId = Guid.NewGuid();
        var pedido = NovaReserva(compradorId, tipo: TipoPedido.Compra);
        _pedidoRepo.GetByIdAsync(pedido.Id).Returns(pedido);

        var resultado = await service.CancelarReservaAsync(pedido.Id, compradorId);

        Assert.Equal(CancelarReservaStatus.Conflito, resultado.Status);
        await _pedidoRepo.DidNotReceive().TryCancelarAsync(Arg.Any<Guid>());
        await _produtoRepo.DidNotReceive().IncrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }

    // ---------- 409: reserva fora de Reservado ----------
    [Theory]
    [InlineData(PedidoStatus.Confirmado)]
    [InlineData(PedidoStatus.Concluido)]
    [InlineData(PedidoStatus.Cancelado)]
    [InlineData(PedidoStatus.Expirado)]
    public async Task Cancelar_QuandoReservaNaoEstaReservada_RetornaConflitoSemGravarNemDevolverEstoque(PedidoStatus status)
    {
        var service = CriarService();
        var compradorId = Guid.NewGuid();
        var pedido = NovaReserva(compradorId, status);
        _pedidoRepo.GetByIdAsync(pedido.Id).Returns(pedido);

        var resultado = await service.CancelarReservaAsync(pedido.Id, compradorId);

        Assert.Equal(CancelarReservaStatus.Conflito, resultado.Status);
        await _pedidoRepo.DidNotReceive().TryCancelarAsync(Arg.Any<Guid>());
        await _produtoRepo.DidNotReceive().IncrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }

    // ---------- 409: corrida (expirou/concluiu/duplo clique entre a leitura e a gravacao) ----------
    [Fact]
    public async Task Cancelar_QuandoGravacaoAtomicaNaoAfetaLinha_RetornaConflitoSemDevolverEstoque()
    {
        var service = CriarService();
        var compradorId = Guid.NewGuid();
        var pedido = NovaReserva(compradorId);
        _pedidoRepo.GetByIdAsync(pedido.Id).Returns(pedido);
        // outro caminho (job de expiracao, vendedor, outro clique) ganhou a corrida
        _pedidoRepo.TryCancelarAsync(pedido.Id).Returns(false);

        var resultado = await service.CancelarReservaAsync(pedido.Id, compradorId);

        Assert.Equal(CancelarReservaStatus.Conflito, resultado.Status);
        // quem perdeu a corrida NAO devolve o estoque (evita devolucao dupla)
        await _produtoRepo.DidNotReceive().IncrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }

    // ---------- 200: caminho feliz, estoque volta uma vez por item ----------
    [Fact]
    public async Task Cancelar_QuandoCompradorDonoEReservaReservada_CancelaEDevolveEstoqueDeCadaItem()
    {
        var service = CriarService();
        var compradorId = Guid.NewGuid();
        var pedido = NovaReserva(compradorId);
        _pedidoRepo.GetByIdAsync(pedido.Id).Returns(pedido);
        _pedidoRepo.TryCancelarAsync(pedido.Id).Returns(true);

        var resultado = await service.CancelarReservaAsync(pedido.Id, compradorId);

        Assert.Equal(CancelarReservaStatus.Sucesso, resultado.Status);
        await _pedidoRepo.Received(1).TryCancelarAsync(pedido.Id);
        // cada item volta ao estoque exatamente na quantidade reservada, uma unica vez
        await _produtoRepo.Received(1).IncrementarEstoqueAsync(ProdutoA, 2);
        await _produtoRepo.Received(1).IncrementarEstoqueAsync(ProdutoB, 1);
        await _produtoRepo.Received(2).IncrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }
}
