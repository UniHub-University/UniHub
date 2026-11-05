using System;
using Xunit;
using UniHub.Domain.Entities;

namespace UniHub.Tests.Application;

// Regra de transicao na entidade: Pedido.Cancelar() (cancelamento de reserva).
[Trait("Categoria", "Unit")]
public class PedidoCancelarTests
{
    [Fact]
    public void Cancelar_QuandoReservaReservada_MudaParaCanceladoEAtualizaUpdatedAt()
    {
        var pedido = new Pedido { Tipo = TipoPedido.Reserva, Status = PedidoStatus.Reservado };

        pedido.Cancelar();

        Assert.Equal(PedidoStatus.Cancelado, pedido.Status);
        Assert.NotNull(pedido.UpdatedAt);
    }

    [Fact]
    public void Cancelar_QuandoCompra_LancaInvalidOperationESemMudarStatus()
    {
        // Compra e definitiva: nao se cancela, mesmo estando Reservado.
        var pedido = new Pedido { Tipo = TipoPedido.Compra, Status = PedidoStatus.Reservado };

        var erro = Assert.Throws<InvalidOperationException>(() => pedido.Cancelar());

        Assert.Contains("compra", erro.Message);
        Assert.Equal(PedidoStatus.Reservado, pedido.Status);
        Assert.Null(pedido.UpdatedAt);
    }

    [Theory]
    [InlineData(PedidoStatus.Confirmado)]
    [InlineData(PedidoStatus.Concluido)]
    [InlineData(PedidoStatus.Cancelado)]
    [InlineData(PedidoStatus.Expirado)]
    public void Cancelar_QuandoReservaForaDeReservado_LancaInvalidOperationESemMudarStatus(PedidoStatus status)
    {
        var pedido = new Pedido { Tipo = TipoPedido.Reserva, Status = status };

        Assert.Throws<InvalidOperationException>(() => pedido.Cancelar());

        Assert.Equal(status, pedido.Status);
        Assert.Null(pedido.UpdatedAt);
    }
}
