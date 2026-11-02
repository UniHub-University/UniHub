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

[Trait("Categoria", "Unit")]
public class PedidoServiceTests
{
    private readonly IProdutoRepository _produtoRepo = Substitute.For<IProdutoRepository>();
    private readonly IPedidoRepository _pedidoRepo = Substitute.For<IPedidoRepository>();

    private PedidoService CriarService() => new(_produtoRepo, _pedidoRepo);

    // Helper: cria um produto concreto (Produto e abstrato) com estado configuravel.
    private static ItemBazar NovoProduto(
        Guid? id = null,
        decimal preco = 10m,
        ProdutoStatus status = ProdutoStatus.Disponivel) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Nome = "Produto Teste",
        Preco = preco,
        Status = status
    };

    // Helper: Alimento (Reserva nao vale para ele).
    private static Alimento NovoAlimento(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Nome = "Salgado Teste",
        Preco = 6m,
        Status = ProdutoStatus.Disponivel
    };

    // ---------- Pedido sem itens ----------
    [Fact]
    public async Task CreatePedido_QuandoListaDeItensVazia_RetornaFalhaSemTocarRepositorios()
    {
        var service = CriarService();
        var request = new CreatePedidoDto(new List<CreateItemPedidoDto>());

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.False(resultado.Sucesso);
        Assert.Equal("Pedido sem itens.", resultado.Mensagem);
        Assert.Null(resultado.PedidoId);
        // nao deve nem consultar produto nem salvar pedido
        await _produtoRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
        await _pedidoRepo.DidNotReceive().AddAsync(Arg.Any<Pedido>());
    }

    // ---------- Produto inexistente / indisponivel ----------
    [Fact]
    public async Task CreatePedido_QuandoProdutoNaoExiste_RetornaIndisponivel()
    {
        var service = CriarService();
        var produtoId = Guid.NewGuid();
        _produtoRepo.GetByIdAsync(produtoId).Returns((Produto?)null);

        var request = new CreatePedidoDto(new List<CreateItemPedidoDto>
        {
            new(produtoId, 1)
        });

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.False(resultado.Sucesso);
        Assert.Contains("indisponível", resultado.Mensagem);
        Assert.Null(resultado.PedidoId);
        // nao chegou a tentar decrementar estoque
        await _produtoRepo.DidNotReceive().TryDecrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }

    [Fact]
    public async Task CreatePedido_QuandoProdutoNaoEstaDisponivel_RetornaIndisponivel()
    {
        var service = CriarService();
        var produto = NovoProduto(status: ProdutoStatus.Esgotado);
        _produtoRepo.GetByIdAsync(produto.Id).Returns(produto);

        var request = new CreatePedidoDto(new List<CreateItemPedidoDto>
        {
            new(produto.Id, 1)
        });

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.False(resultado.Sucesso);
        Assert.Contains("indisponível", resultado.Mensagem);
        await _produtoRepo.DidNotReceive().TryDecrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
    }

    // ---------- Estoque insuficiente ----------
    [Fact]
    public async Task CreatePedido_QuandoEstoqueInsuficiente_RetornaFalha()
    {
        var service = CriarService();
        var produto = NovoProduto();
        _produtoRepo.GetByIdAsync(produto.Id).Returns(produto);
        // o decremento atomico falha (ex.: outra pessoa comprou o ultimo)
        _produtoRepo.TryDecrementarEstoqueAsync(produto.Id, 2).Returns(false);

        var request = new CreatePedidoDto(new List<CreateItemPedidoDto>
        {
            new(produto.Id, 2)
        });

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Estoque insuficiente", resultado.Mensagem);
        Assert.Null(resultado.PedidoId);
        // como foi o primeiro e unico item, nada foi decrementado antes -> nada a estornar
        await _produtoRepo.DidNotReceive().IncrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
        await _pedidoRepo.DidNotReceive().AddAsync(Arg.Any<Pedido>());
    }

    // ---------- Caminho feliz ----------
    [Fact]
    public async Task CreatePedido_QuandoTudoOk_CriaPedidoERetornaSucesso()
    {
        var service = CriarService();
        var produto = NovoProduto(preco: 25m);
        _produtoRepo.GetByIdAsync(produto.Id).Returns(produto);
        _produtoRepo.TryDecrementarEstoqueAsync(produto.Id, 3).Returns(true);

        // AddAsync devolve o proprio pedido recebido (com Id), simulando persistencia
        _pedidoRepo.AddAsync(Arg.Any<Pedido>()).Returns(call => call.Arg<Pedido>());

        var request = new CreatePedidoDto(new List<CreateItemPedidoDto>
        {
            new(produto.Id, 3)
        });

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Pedido reservado com sucesso.", resultado.Mensagem);
        Assert.NotNull(resultado.PedidoId);
        // salvou exatamente um pedido, com o item correto e o preco congelado
        await _pedidoRepo.Received(1).AddAsync(Arg.Is<Pedido>(p =>
            p.Itens.Count == 1 &&
            p.Itens.First().ProdutoId == produto.Id &&
            p.Itens.First().Quantidade == 3 &&
            p.Itens.First().PrecoUnitarioNaCompra == 25m));
    }

    // ---------- Rollback: segundo item falha, primeiro e estornado ----------
    [Fact]
    public async Task CreatePedido_QuandoSegundoItemFalha_EstornaOPrimeiroItem()
    {
        var service = CriarService();

        var produtoA = NovoProduto();
        var produtoB = NovoProduto();
        _produtoRepo.GetByIdAsync(produtoA.Id).Returns(produtoA);
        _produtoRepo.GetByIdAsync(produtoB.Id).Returns(produtoB);

        // A decrementa com sucesso; B falha por falta de estoque
        _produtoRepo.TryDecrementarEstoqueAsync(produtoA.Id, 1).Returns(true);
        _produtoRepo.TryDecrementarEstoqueAsync(produtoB.Id, 1).Returns(false);

        var request = new CreatePedidoDto(new List<CreateItemPedidoDto>
        {
            new(produtoA.Id, 1),
            new(produtoB.Id, 1)
        });

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Estoque insuficiente", resultado.Mensagem);
        // o item A (ja decrementado) deve ser estornado exatamente na mesma quantidade
        await _produtoRepo.Received(1).IncrementarEstoqueAsync(produtoA.Id, 1);
        // B nunca foi decrementado, entao nao pode ser estornado
        await _produtoRepo.DidNotReceive().IncrementarEstoqueAsync(produtoB.Id, Arg.Any<int>());
        // pedido nao e salvo quando ha falha
        await _pedidoRepo.DidNotReceive().AddAsync(Arg.Any<Pedido>());
    }

    // ---------- Comprar vs Reservar (P2/P3) ----------
    [Fact]
    public async Task CreatePedido_SemTipo_ECompraSemExpiracao()
    {
        var service = CriarService();
        var produto = NovoProduto();
        _produtoRepo.GetByIdAsync(produto.Id).Returns(produto);
        _produtoRepo.TryDecrementarEstoqueAsync(produto.Id, 1).Returns(true);
        _pedidoRepo.AddAsync(Arg.Any<Pedido>()).Returns(call => call.Arg<Pedido>());

        // sem informar o Tipo: o padrao do DTO e Compra
        var request = new CreatePedidoDto(new List<CreateItemPedidoDto> { new(produto.Id, 1) });

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.True(resultado.Sucesso);
        await _pedidoRepo.Received(1).AddAsync(Arg.Is<Pedido>(p =>
            p.Tipo == TipoPedido.Compra && p.ExpiraEm == null));
    }

    [Fact]
    public async Task CreatePedido_ReservaDeItemBazar_GravaTipoEExpiraEm()
    {
        var service = CriarService();
        var produto = NovoProduto();
        _produtoRepo.GetByIdAsync(produto.Id).Returns(produto);
        _produtoRepo.TryDecrementarEstoqueAsync(produto.Id, 1).Returns(true);

        Pedido? salvo = null;
        _pedidoRepo.AddAsync(Arg.Any<Pedido>()).Returns(call =>
        {
            var pedido = call.Arg<Pedido>();
            salvo = pedido;
            return pedido;
        });

        var request = new CreatePedidoDto(
            new List<CreateItemPedidoDto> { new(produto.Id, 1) },
            TipoPedido.Reserva);

        var antes = DateTime.UtcNow;
        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);
        var depois = DateTime.UtcNow;

        Assert.True(resultado.Sucesso);
        Assert.NotNull(salvo);
        Assert.Equal(TipoPedido.Reserva, salvo!.Tipo);
        Assert.Equal(PedidoStatus.Reservado, salvo.Status);
        // ExpiraEm = agora + TTL (dentro da janela em que o teste rodou)
        Assert.NotNull(salvo.ExpiraEm);
        Assert.InRange(salvo.ExpiraEm!.Value, antes.Add(PedidoService.TtlReserva), depois.Add(PedidoService.TtlReserva));
    }

    [Fact]
    public async Task CreatePedido_ReservaDeAlimento_RetornaFalhaSemDecrementarEstoque()
    {
        var service = CriarService();
        var alimento = NovoAlimento();
        _produtoRepo.GetByIdAsync(alimento.Id).Returns(alimento);

        var request = new CreatePedidoDto(
            new List<CreateItemPedidoDto> { new(alimento.Id, 1) },
            TipoPedido.Reserva);

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.False(resultado.Sucesso);
        Assert.Contains("nao pode ser reservado", resultado.Mensagem);
        Assert.Null(resultado.PedidoId);
        await _produtoRepo.DidNotReceive().TryDecrementarEstoqueAsync(Arg.Any<Guid>(), Arg.Any<int>());
        await _pedidoRepo.DidNotReceive().AddAsync(Arg.Any<Pedido>());
    }

    [Fact]
    public async Task CreatePedido_ReservaComAlimentoNoSegundoItem_EstornaOPrimeiro()
    {
        var service = CriarService();
        var itemBazar = NovoProduto();
        var alimento = NovoAlimento();
        _produtoRepo.GetByIdAsync(itemBazar.Id).Returns(itemBazar);
        _produtoRepo.GetByIdAsync(alimento.Id).Returns(alimento);
        _produtoRepo.TryDecrementarEstoqueAsync(itemBazar.Id, 1).Returns(true);

        var request = new CreatePedidoDto(
            new List<CreateItemPedidoDto> { new(itemBazar.Id, 1), new(alimento.Id, 1) },
            TipoPedido.Reserva);

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.False(resultado.Sucesso);
        // o item do Bazar ja decrementado volta ao estoque; o alimento nunca foi tocado
        await _produtoRepo.Received(1).IncrementarEstoqueAsync(itemBazar.Id, 1);
        await _produtoRepo.DidNotReceive().TryDecrementarEstoqueAsync(alimento.Id, Arg.Any<int>());
        await _pedidoRepo.DidNotReceive().AddAsync(Arg.Any<Pedido>());
    }

    [Fact]
    public async Task CreatePedido_CompraDeAlimento_ContinuaPermitida()
    {
        var service = CriarService();
        var alimento = NovoAlimento();
        _produtoRepo.GetByIdAsync(alimento.Id).Returns(alimento);
        _produtoRepo.TryDecrementarEstoqueAsync(alimento.Id, 1).Returns(true);
        _pedidoRepo.AddAsync(Arg.Any<Pedido>()).Returns(call => call.Arg<Pedido>());

        var request = new CreatePedidoDto(
            new List<CreateItemPedidoDto> { new(alimento.Id, 1) },
            TipoPedido.Compra);

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.True(resultado.Sucesso);
    }

    [Fact]
    public async Task CreatePedido_QuandoTipoInvalido_RetornaFalhaSemTocarRepositorios()
    {
        var service = CriarService();
        var request = new CreatePedidoDto(
            new List<CreateItemPedidoDto> { new(Guid.NewGuid(), 1) },
            (TipoPedido)99);

        var resultado = await service.CreatePedidoAsync(Guid.NewGuid(), request);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Tipo de pedido invalido", resultado.Mensagem);
        await _produtoRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
        await _pedidoRepo.DidNotReceive().AddAsync(Arg.Any<Pedido>());
    }


    // ---------- Historico com Tipo e ExpiraEm (extensao Sprint 4) ----------
    [Fact]
    public async Task GetHistorico_PedidoDeCompra_DevolveTipoCompraSemExpiraEm()
    {
        var service = CriarService();
        var compradorId = Guid.NewGuid();
        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            CompradorId = compradorId,
            Status = PedidoStatus.Reservado,
            Tipo = TipoPedido.Compra,
            ExpiraEm = null,
            CreatedAt = DateTime.UtcNow,
            Itens = new List<ItemPedido>()
        };
        var lista = new List<Pedido> { pedido };
        _pedidoRepo.GetByCompradorAsync(compradorId, 1, 10)
            .Returns(((IReadOnlyList<Pedido>)lista, lista.Count));

        var (pedidos, total, _, _) = await service.GetHistoricoAsync(compradorId, 1, 10);

        Assert.Equal(1, total);
        var dto = Assert.Single(pedidos);
        Assert.Equal(TipoPedido.Compra, dto.Tipo);
        Assert.Null(dto.ExpiraEm);
    }

    [Fact]
    public async Task GetHistorico_PedidoDeReserva_DevolveTipoReservaEExpiraEm()
    {
        var service = CriarService();
        var compradorId = Guid.NewGuid();
        var expiraEm = DateTime.UtcNow.AddHours(24);
        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            CompradorId = compradorId,
            Status = PedidoStatus.Reservado,
            Tipo = TipoPedido.Reserva,
            ExpiraEm = expiraEm,
            CreatedAt = DateTime.UtcNow,
            Itens = new List<ItemPedido>()
        };
        var lista = new List<Pedido> { pedido };
        _pedidoRepo.GetByCompradorAsync(compradorId, 1, 10)
            .Returns(((IReadOnlyList<Pedido>)lista, lista.Count));

        var (pedidos, _, _, _) = await service.GetHistoricoAsync(compradorId, 1, 10);

        var dto = Assert.Single(pedidos);
        Assert.Equal(TipoPedido.Reserva, dto.Tipo);
        Assert.Equal(expiraEm, dto.ExpiraEm);
    }


}
