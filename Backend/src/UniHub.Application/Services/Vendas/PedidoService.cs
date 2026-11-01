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

    // Quanto tempo uma Reserva vale antes de expirar (ExpiraEm = agora + TTL).
    // Publico para o job de expiracao (P4) reutilizar o mesmo valor.
    // 24h e o valor inicial proposto; a confirmar com o squad.
    public static readonly TimeSpan TtlReserva = TimeSpan.FromHours(24);

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

        // Enum vindo da API: nunca assumir que o inteiro recebido e um valor valido.
        if (!Enum.IsDefined(request.Tipo))
        {
            return new PedidoResultDto(false, "Tipo de pedido invalido.", null);
        }

        var pedido = new Pedido
        {
            CompradorId = compradorId,
            Status = PedidoStatus.Reservado,
            Tipo = request.Tipo,
            // So a Reserva expira; a Compra e definitiva (ExpiraEm nulo).
            ExpiraEm = request.Tipo == TipoPedido.Reserva ? DateTime.UtcNow.Add(TtlReserva) : null,
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

            // Reserva e temporaria e so vale para ItemBazar (nao para Alimento).
            if (request.Tipo == TipoPedido.Reserva && produto is not ItemBazar)
            {
                await RollbackAsync(decrementados);
                return new PedidoResultDto(false, $"Produto '{produto.Nome}' nao pode ser reservado: reserva so vale para itens do Bazar.", null);
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

    // Historico de pedidos:
    private const int PageSizePadrao = 10;
    private const int PageSizeMaximo = 50;

    // Devolve a pagina do historico do comprador autenticado (compradorId vem do
    // token no Controller, nunca da requisicao) e os valores de page/pageSize
    // efetivamente usados, para o Controller montar a resposta paginada.
    public async Task<(List<HistoricoPedidoDto> Pedidos, int Total, int Page, int PageSize)> GetHistoricoAsync(
        Guid compradorId, int page, int pageSize)
    {
        // Valida a entrada: nunca confiar em page/pageSize vindos da requisicao.
        pageSize = pageSize < 1 ? PageSizePadrao : Math.Min(pageSize, PageSizeMaximo);

        // O teto de page evita estouro de int em (page - 1) * pageSize no repositorio.
        page = Math.Clamp(page, 1, int.MaxValue / pageSize);

        var (pedidos, total) = await _pedidoRepository.GetByCompradorAsync(compradorId, page, pageSize);

        var historico = pedidos
            .Select(p => new HistoricoPedidoDto(
                p.Id,
                p.Status,
                p.CreatedAt,
                p.Itens
                    .Select(i => new HistoricoItemPedidoDto(
                        i.ProdutoId,
                        i.Produto?.Nome ?? "Produto indisponivel",
                        i.Quantidade,
                        i.PrecoUnitarioNaCompra))
                    .ToList(),
                p.Itens.Sum(i => i.Quantidade * i.PrecoUnitarioNaCompra)))
            .ToList();

        return (historico, total, page, pageSize);
    }

    private async Task RollbackAsync (List<(Guid ProdutoId, int Quantidade)> decrementados)
    {
        foreach (var (produtoId, quantidade) in decrementados)
        {
            await _produtoRepository.IncrementarEstoqueAsync(produtoId, quantidade);
        }
    }
}