using System;
using System.Threading.Tasks;
using NSubstitute;
using UniHub.Application.DTOs.Vendas;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Application.Services.Vendas;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Application;

// Testes do P7: baixa manual do pedido pelo vendedor.
[Trait("Categoria", "Unit")]
public class PedidoServiceConcluirTests
{
    private readonly IProdutoRepository _produtoRepo = Substitute.For<IProdutoRepository>();
    private readonly IPedidoRepository _pedidoRepo = Substitute.For<IPedidoRepository>();

    private PedidoService CriarService() => new(_produtoRepo, _pedidoRepo);

    private static Pedido NovoPedido(PedidoStatus status = PedidoStatus.Reservado) => new()
    {
        Id = Guid.NewGuid(),
        CompradorId = Guid.NewGuid(),
        Status = status
    };

    // ---------- 404: pedido inexistente ----------
    [Fact]
    public async Task Concluir_QuandoPedidoNaoExiste_RetornaNaoEncontrado()
    {
        var service = CriarService();
        var pedidoId = Guid.NewGuid();
        _pedidoRepo.GetByIdAsync(pedidoId).Returns((Pedido?)null);

        var resultado = await service.ConcluirPedidoAsync(pedidoId, Guid.NewGuid());

        Assert.Equal(ConcluirPedidoStatus.NaoEncontrado, resultado.Status);
        await _pedidoRepo.DidNotReceive().PedidoTemItemDoVendedorAsync(Arg.Any<Guid>(), Arg.Any<Guid>());
        await _pedidoRepo.DidNotReceive().TryConcluirAsync(Arg.Any<Guid>());
    }

    // ---------- 403: usuario nao e vendedor de nenhum item ----------
    [Fact]
    public async Task Concluir_QuandoUsuarioNaoEVendedorDoPedido_RetornaProibidoSemGravar()
    {
        var service = CriarService();
        var pedido = NovoPedido();
        var usuarioId = Guid.NewGuid();
        _pedidoRepo.GetByIdAsync(pedido.Id).Returns(pedido);
        _pedidoRepo.PedidoTemItemDoVendedorAsync(pedido.Id, usuarioId).Returns(false);

        var resultado = await service.ConcluirPedidoAsync(pedido.Id, usuarioId);

        Assert.Equal(ConcluirPedidoStatus.Proibido, resultado.Status);
        await _pedidoRepo.DidNotReceive().TryConcluirAsync(Arg.Any<Guid>());
    }

    // ---------- 409: pedido fora de Reservado ----------
    [Theory]
    [InlineData(PedidoStatus.Confirmado)]
    [InlineData(PedidoStatus.Concluido)]
    [InlineData(PedidoStatus.Cancelado)]
    [InlineData(PedidoStatus.Expirado)]
    public async Task Concluir_QuandoPedidoNaoEstaReservado_RetornaConflitoSemGravar(PedidoStatus status)
    {
        var service = CriarService();
        var pedido = NovoPedido(status);
        var usuarioId = Guid.NewGuid();
        _pedidoRepo.GetByIdAsync(pedido.Id).Returns(pedido);
        _pedidoRepo.PedidoTemItemDoVendedorAsync(pedido.Id, usuarioId).Returns(true);

        var resultado = await service.ConcluirPedidoAsync(pedido.Id, usuarioId);

        Assert.Equal(ConcluirPedidoStatus.Conflito, resultado.Status);
        await _pedidoRepo.DidNotReceive().TryConcluirAsync(Arg.Any<Guid>());
    }

    // ---------- 409: corrida (o pedido mudou entre a leitura e a gravacao) ----------
    [Fact]
    public async Task Concluir_QuandoGravacaoAtomicaNaoAfetaLinha_RetornaConflito()
    {
        var service = CriarService();
        var pedido = NovoPedido();
        var usuarioId = Guid.NewGuid();
        _pedidoRepo.GetByIdAsync(pedido.Id).Returns(pedido);
        _pedidoRepo.PedidoTemItemDoVendedorAsync(pedido.Id, usuarioId).Returns(true);
        // ex.: o job de expiracao mudou o pedido para Expirado entre a leitura e o UPDATE
        _pedidoRepo.TryConcluirAsync(pedido.Id).Returns(false);

        var resultado = await service.ConcluirPedidoAsync(pedido.Id, usuarioId);

        Assert.Equal(ConcluirPedidoStatus.Conflito, resultado.Status);
    }

    // ---------- 200: caminho feliz ----------
    [Fact]
    public async Task Concluir_QuandoVendedorDonoEPedidoReservado_ConcluiComSucesso()
    {
        var service = CriarService();
        var pedido = NovoPedido();
        var usuarioId = Guid.NewGuid();
        _pedidoRepo.GetByIdAsync(pedido.Id).Returns(pedido);
        _pedidoRepo.PedidoTemItemDoVendedorAsync(pedido.Id, usuarioId).Returns(true);
        _pedidoRepo.TryConcluirAsync(pedido.Id).Returns(true);

        var resultado = await service.ConcluirPedidoAsync(pedido.Id, usuarioId);

        Assert.Equal(ConcluirPedidoStatus.Sucesso, resultado.Status);
        await _pedidoRepo.Received(1).TryConcluirAsync(pedido.Id);
    }
}

// Regra de transicao na entidade (Pedido.Concluir).
[Trait("Categoria", "Unit")]
public class PedidoConcluirTests
{
    [Fact]
    public void Concluir_QuandoReservado_MudaParaConcluidoEAtualizaUpdatedAt()
    {
        var pedido = new Pedido { Status = PedidoStatus.Reservado };

        pedido.Concluir();

        Assert.Equal(PedidoStatus.Concluido, pedido.Status);
        Assert.NotNull(pedido.UpdatedAt);
    }

    [Theory]
    [InlineData(PedidoStatus.Confirmado)]
    [InlineData(PedidoStatus.Concluido)]
    [InlineData(PedidoStatus.Cancelado)]
    [InlineData(PedidoStatus.Expirado)]
    public void Concluir_QuandoNaoReservado_LancaInvalidOperationESemMudarStatus(PedidoStatus status)
    {
        var pedido = new Pedido { Status = status };

        Assert.Throws<InvalidOperationException>(() => pedido.Concluir());

        Assert.Equal(status, pedido.Status);
        Assert.Null(pedido.UpdatedAt);
    }
}