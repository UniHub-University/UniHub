using UniHub.Application.DTOs.Vendas;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Domain.Entities;

namespace UniHub.Application.Services.Vendas;

// Aqui ocorre a orquestracao da criacao de pedidos.
// A Resolucao da concorrencia se da no repositorio (IPedidoRepository), este servico apenas
// reage ao resultado.

public class PedidoService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly IPedidoRepository _pedidoRepository;


    public PedidoService(IProdutoRepository produtoRepository, IPedidoRepository pedidoRepository)
    {
        _produtoRepository = produtoRepository;
        _pedidoRepository = pedidoRepository;
    }

    public async Task<PedidoResultDto> CreatePedidoAsync(Guid compradorId, CreatePedidoDto request)
    {
        if (request.Itens is null || request.Itens.Count == 0)
        {
            return new PedidoResultDto(false, "Pedido sem itens.", null);
        }

        var pedido = new Pedido
        {
            CompradorId = compradorId,
            Status = PedidoStatus.Reservado,
            Itens = new List<ItemPedido>()
        };

        // tenta reservar cada item. Se algum item falhar por falta de estoque,
        // interrompe e devolve os itens já decrementados com sucesso

        var decrementados = new List<(Guid ProdutoId, int Quantidade)>();

        foreach (var item in request.Itens)
        {
            var produto = await _produtoRepository.GetByIdAsync(item.ProdutoId);
            if (produto is null || produto.Status != ProdutoStatus.Disponivel)
            {
                 await RollbackAsync(decrementados);
                 return new PedidoResultDto(false, $"Produto {item.ProdutoId} indisponível.", null);
            }

            var sucesso = await _produtoRepository.TryDecrementarEstoqueAsync(item.ProdutoId, item.Quantidade);
            if (!sucesso)
            {
                // caso de duas pessoas tentando comprar o último item do estoque ao mesmo tempo
                // quem faz a solicitação por ultimo, cai aqui
                await RollbackAsync(decrementados);
                return new PedidoResultDto(false, $"Estoque insuficiente para '{produto.Nome}'.", null);

            }

            decrementados.Add((item.ProdutoId, item.Quantidade));
            pedido.Itens.Add(new ItemPedido
            {
                ProdutoId = produto.Id,
                Quantidade = item.Quantidade,
                PrecoUnitarioNaCompra = produto.Preco
            });
        }

        var salvo = await _pedidoRepository.AddAsync(pedido);
        return new PedidoResultDto(true, "Pedido reservado com sucesso.", salvo.Id);

    }

    private async Task RollbackAsync (List<(Guid ProdutoId, int Quantidade)> decrementados)
    {
        foreach (var (produtoId, quantidade) in decrementados)
        {
            await _produtoRepository.IncrementarEstoqueAsync(produtoId, quantidade);
        }
    }
}