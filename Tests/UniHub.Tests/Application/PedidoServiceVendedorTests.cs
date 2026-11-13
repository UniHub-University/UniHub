using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Application.Services.Vendas;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Application;

// Testes do P6: lista de pedidos do vendedor.
[Trait("Categoria", "Unit")]
public class PedidoServiceVendedorTests
{
    private readonly IProdutoRepository _produtoRepo = Substitute.For<IProdutoRepository>();
    private readonly IPedidoRepository _pedidoRepo = Substitute.For<IPedidoRepository>();
    private readonly INotificadorPainel _notificadorPainel = Substitute.For<INotificadorPainel>();

    private PedidoService CriarService() => new(_produtoRepo, _pedidoRepo, _notificadorPainel);

    // Produto cujo Vendedor pertence ao usuario informado (Vendedor.UsuarioId).
    private static ItemBazar NovoProdutoDoVendedor(Guid usuarioDoVendedor, string nome = "Produto", decimal preco = 10m)
    {
        var vendedor = new Vendedor { UsuarioId = usuarioDoVendedor };
        return new ItemBazar
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Preco = preco,
            Vendedor = vendedor,
            VendedorId = vendedor.Id
        };
    }

    private static ItemPedido NovoItem(Produto produto, int quantidade, decimal preco) => new()
    {
        ProdutoId = produto.Id,
        Produto = produto,
        Quantidade = quantidade,
        PrecoUnitarioNaCompra = preco
    };

    private void RepoDevolve(Guid usuarioId, int page, int pageSize, List<Pedido> pedidos) =>
        _pedidoRepo.GetByVendedorAsync(usuarioId, page, pageSize)
            .Returns(((IReadOnlyList<Pedido>)pedidos, pedidos.Count));

    // ---------- So os itens do vendedor, e o total dessa parte ----------
    [Fact]
    public async Task GetPedidosDoVendedor_PedidoComItensDeDoisVendedores_MostraSoOsDoVendedorETotalDele()
    {
        var service = CriarService();
        var usuarioVendedor = Guid.NewGuid();
        var meuProduto = NovoProdutoDoVendedor(usuarioVendedor, "Jaqueta", 100m);
        var produtoDeOutro = NovoProdutoDoVendedor(Guid.NewGuid(), "Camiseta", 30m);

        var pedido = new Pedido
        {
            Comprador = new Usuario { NomeCompleto = "Maria Silva" },
            Itens = new List<ItemPedido>
            {
                NovoItem(meuProduto, 2, 100m),
                NovoItem(produtoDeOutro, 1, 30m)
            }
        };
        RepoDevolve(usuarioVendedor, 1, 10, new List<Pedido> { pedido });

        var (pedidos, _, _, _) = await service.GetPedidosDoVendedorAsync(usuarioVendedor, 1, 10);

        var dto = Assert.Single(pedidos);
        var item = Assert.Single(dto.Itens);
        Assert.Equal("Jaqueta", item.ProdutoNome);
        Assert.Equal(2, item.Quantidade);
        // total so da parte do vendedor: 2 x 100 (a camiseta do outro vendedor fica de fora)
        Assert.Equal(200m, dto.Total);
    }

    // ---------- Comprador: so o nome ----------
    [Fact]
    public async Task GetPedidosDoVendedor_MostraNomeDoCompradorETipoEExpiraEm()
    {
        var service = CriarService();
        var usuarioVendedor = Guid.NewGuid();
        var produto = NovoProdutoDoVendedor(usuarioVendedor);
        var expiraEm = DateTime.UtcNow.AddHours(24);

        var pedido = new Pedido
        {
            Tipo = TipoPedido.Reserva,
            ExpiraEm = expiraEm,
            Comprador = new Usuario
            {
                NomeCompleto = "Maria Silva",
                EmailInstitucional = "maria@unifesp.br",
                Telefone = "12999999999"
            },
            Itens = new List<ItemPedido> { NovoItem(produto, 1, 10m) }
        };
        RepoDevolve(usuarioVendedor, 1, 10, new List<Pedido> { pedido });

        var (pedidos, _, _, _) = await service.GetPedidosDoVendedorAsync(usuarioVendedor, 1, 10);

        var dto = Assert.Single(pedidos);
        Assert.Equal("Maria Silva", dto.CompradorNome);
        Assert.Equal(TipoPedido.Reserva, dto.Tipo);
        Assert.Equal(expiraEm, dto.ExpiraEm);
    }

    // ---------- Paginacao ----------
    [Fact]
    public async Task GetPedidosDoVendedor_AjustaPaginaETamanhoInvalidos()
    {
        var service = CriarService();
        var usuarioVendedor = Guid.NewGuid();
        // page 0 vira 1; pageSize 1000 vira o maximo (50)
        RepoDevolve(usuarioVendedor, 1, 50, new List<Pedido>());

        var (_, _, paginaUsada, tamanhoUsado) = await service.GetPedidosDoVendedorAsync(usuarioVendedor, 0, 1000);

        Assert.Equal(1, paginaUsada);
        Assert.Equal(50, tamanhoUsado);
        await _pedidoRepo.Received(1).GetByVendedorAsync(usuarioVendedor, 1, 50);
    }

    // ---------- Sem pedidos (ex.: quem nao e vendedor) ----------
    [Fact]
    public async Task GetPedidosDoVendedor_QuandoNaoHaPedidos_DevolveListaVazia()
    {
        var service = CriarService();
        var usuarioId = Guid.NewGuid();
        RepoDevolve(usuarioId, 1, 10, new List<Pedido>());

        var (pedidos, total, _, _) = await service.GetPedidosDoVendedorAsync(usuarioId, 1, 10);

        Assert.Empty(pedidos);
        Assert.Equal(0, total);
    }

    // ---------- O vendedor consultado e sempre o usuario recebido (do token) ----------
    [Fact]
    public async Task GetPedidosDoVendedor_ConsultaORepositorioComOUsuarioInformado()
    {
        var service = CriarService();
        var usuarioId = Guid.NewGuid();
        RepoDevolve(usuarioId, 1, 10, new List<Pedido>());

        await service.GetPedidosDoVendedorAsync(usuarioId, 1, 10);

        await _pedidoRepo.Received(1).GetByVendedorAsync(usuarioId, Arg.Any<int>(), Arg.Any<int>());
    }
}