using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Application.Services.Vendas;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Application;

// Testes do P4/P5: expiracao de reservas (devolve estoque + marca Expirado),
// idempotencia da devolucao de estoque e a checagem lazy nos GETs.
[Trait("Categoria", "Unit")]
public class PedidoServiceExpiracaoTests
{
    private readonly IProdutoRepository _produtoRepo = Substitute.For<IProdutoRepository>();
    private readonly IPedidoRepository _pedidoRepo = Substitute.For<IPedidoRepository>();

    private PedidoService CriarService() => new(_produtoRepo, _pedidoRepo);

    private static Pedido ReservaVencida(params (Guid produtoId, int qtd)[] itens)
    {
        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            CompradorId = Guid.NewGuid(),
            Tipo = TipoPedido.Reserva,
            Status = PedidoStatus.Reservado,
            ExpiraEm = DateTime.UtcNow.AddHours(-1), // ja venceu
            CreatedAt = DateTime.UtcNow.AddHours(-49)
        };
        foreach (var (produtoId, qtd) in itens)
        {
            pedido.Itens.Add(new ItemPedido { ProdutoId = produtoId, Quantidade = qtd, PrecoUnitarioNaCompra = 10m });
        }
        return pedido;
    }

    // ---------- P4: job expira e devolve estoque ----------
    [Fact]
    public async Task ExpirarReservasVencidas_QuandoHaReservaVencida_ExpiraEDevolveEstoque()
    {
        var produtoId = Guid.NewGuid();
        var reserva = ReservaVencida((produtoId, 3));
        _pedidoRepo.GetReservasVencidasAsync(Arg.Any<DateTime>())
            .Returns(new List<Pedido> { reserva });
        _pedidoRepo.TryExpirarAsync(reserva.Id).Returns(true); // este chamador "ganhou" a corrida

        var expiradas = await CriarService().ExpirarReservasVencidasAsync();

        Assert.Equal(1, expiradas);
        // devolveu exatamente a quantidade reservada do item
        await _produtoRepo.Received(1).IncrementarEstoqueAsync(produtoId, 3);
    }

    // ---------- Idempotencia: se outro ja expirou, NAO devolve estoque ----------
    [Fact]
    public async Task ExpirarReservasVencidas_QuandoOutroJaExpirou_NaoDevolveEstoque()
    {
        var produtoId = Guid.NewGuid();
        var reserva = ReservaVencida((produtoId, 2));
        _pedidoRepo.GetReservasVencidasAsync(Arg.Any<DateTime>())
            .Returns(new List<Pedido> { reserva });
        // UPDATE condicional nao afetou linha -> corrida perdida (ja expirado/concluido)
        _pedidoRepo.TryExpirarAsync(reserva.Id).Returns(false);

        var expiradas = await CriarService().ExpirarReservasVencidasAsync();

        Assert.Equal(0, expiradas);
        // critico: estoque NAO pode ser devolvido duas vezes
        await _produtoRepo.DidNotReceive().IncrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }

    [Fact]
    public async Task ExpirarReservasVencidas_QuandoNaoHaVencidas_NaoFazNada()
    {
        _pedidoRepo.GetReservasVencidasAsync(Arg.Any<DateTime>())
            .Returns(new List<Pedido>());

        var expiradas = await CriarService().ExpirarReservasVencidasAsync();

        Assert.Equal(0, expiradas);
        await _pedidoRepo.DidNotReceive().TryExpirarAsync(Arg.Any<Guid>());
        await _produtoRepo.DidNotReceive().IncrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }

    // ---------- P5 (lazy): historico reflete Expirado e devolve estoque ----------
    [Fact]
    public async Task GetHistorico_QuandoReservaVencida_RefleteExpiradoEDevolveEstoque()
    {
        var compradorId = Guid.NewGuid();
        var produtoId = Guid.NewGuid();
        var reserva = ReservaVencida((produtoId, 1));
        reserva.CompradorId = compradorId;

        _pedidoRepo.GetByCompradorAsync(compradorId, 1, 10)
            .Returns(((IReadOnlyList<Pedido>)new List<Pedido> { reserva }, 1));
        _pedidoRepo.TryExpirarAsync(reserva.Id).Returns(true);

        var (pedidos, _, _, _) = await CriarService().GetHistoricoAsync(compradorId, 1, 10);

        var dto = Assert.Single(pedidos);
        Assert.Equal(PedidoStatus.Expirado, dto.Status); // lazy refletiu no DTO
        await _produtoRepo.Received(1).IncrementarEstoqueAsync(produtoId, 1);
    }

    // ---------- Compra nunca expira ----------
    [Fact]
    public async Task GetHistorico_QuandoCompra_NaoExpiraMesmoComDataPassada()
    {
        var compradorId = Guid.NewGuid();
        var compra = new Pedido
        {
            Id = Guid.NewGuid(),
            CompradorId = compradorId,
            Tipo = TipoPedido.Compra,
            Status = PedidoStatus.Reservado,
            ExpiraEm = null, // compra nao tem expiracao
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        compra.Itens.Add(new ItemPedido { ProdutoId = Guid.NewGuid(), Quantidade = 1, PrecoUnitarioNaCompra = 10m });

        _pedidoRepo.GetByCompradorAsync(compradorId, 1, 10)
            .Returns(((IReadOnlyList<Pedido>)new List<Pedido> { compra }, 1));

        var (pedidos, _, _, _) = await CriarService().GetHistoricoAsync(compradorId, 1, 10);

        var dto = Assert.Single(pedidos);
        Assert.NotEqual(PedidoStatus.Expirado, dto.Status);
        await _pedidoRepo.DidNotReceive().TryExpirarAsync(Arg.Any<Guid>());
        await _produtoRepo.DidNotReceive().IncrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }
}
