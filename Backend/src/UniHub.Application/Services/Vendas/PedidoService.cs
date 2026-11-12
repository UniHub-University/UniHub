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
    private readonly INotificadorPainel _notificadorPainel;

    // Quanto tempo uma Reserva vale antes de expirar (ExpiraEm = agora + TTL).
    // Publico para o job de expiracao (P4) reutilizar o mesmo valor.
    public static readonly TimeSpan TtlReserva = TimeSpan.FromHours(48);

    public PedidoService(
        IProdutoRepository produtoRepository,
        IPedidoRepository pedidoRepository,
        INotificadorPainel notificadorPainel)
    {
        _produtoRepository = produtoRepository;
        _pedidoRepository = pedidoRepository;
        _notificadorPainel = notificadorPainel;
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

        // Dados para notificar o painel APOS o sucesso (nunca durante o loop, para
        // nao notificar uma mudanca que um rollback posterior desfaria). Uma unica
        // lista alimenta os dois eventos (EstoqueAtualizado e NovoPedido).
        var paraNotificar = new List<(Guid VendedorId, EstoqueAtualizadoDto Estoque, Guid ProdutoId, string NomeProduto, int Quantidade)>();

        foreach (var item in request.Itens)
        {
            var produto = await _produtoRepository.GetByIdAsync(item.ProdutoId);
            if (produto is null || produto.Status != ProdutoStatus.Disponivel)
            {
                 await RollbackAsync(decrementados);
                 return new PedidoResultDto(false, $"Produto {item.ProdutoId} indisponível.", null);
            }

            // Ninguem compra ou reserva o proprio produto. A checagem e no backend
            // (a trava real), nao so no front.
            if (await _produtoRepository.ProdutoPertenceAoUsuarioAsync(item.ProdutoId, compradorId))
            {
                await RollbackAsync(decrementados);
                return new PedidoResultDto(false, $"Você não pode comprar o próprio produto '{produto.Nome}'.", null);
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

            // Quantidade resultante (o decremento e atomico no banco; o service ja
            // conhece a anterior e a decrementada -- AGENTS Secao 7, sem ida extra).
            var quantidadeResultante = produto.QuantidadeDisponivel - item.Quantidade;

            // Status do evento: o decremento NAO altera o Status no banco, mas o
            // painel deve refletir "Esgotado" quando zera (decisao de produto).
            // Derivamos so no payload do evento -- a regra de estoque (Secao 7) nao muda.
            var statusEvento = quantidadeResultante <= 0 ? ProdutoStatus.Esgotado : produto.Status;

            paraNotificar.Add((
                produto.VendedorId,
                new EstoqueAtualizadoDto(produto.Id, quantidadeResultante, statusEvento),
                produto.Id,
                produto.Nome,
                item.Quantidade));
        }

        var salvo = await _pedidoRepository.AddAsync(pedido);

        // Notifica o painel do vendedor em tempo real. Uma falha do hub NUNCA pode
        // derrubar a venda (ja persistida) -- por isso o try/catch por evento.
        // QuandoUtc usa o carimbo real do pedido persistido (CreatedAt, em UTC).
        foreach (var n in paraNotificar)
        {
            try
            {
                await _notificadorPainel.NotificarEstoqueAsync(n.VendedorId, n.Estoque);
            }
            catch
            {
                // Best-effort: o painel se reconcilia no proximo carregamento REST.
                // (Logging estruturado pode ser adicionado quando houver ILogger aqui.)
            }

            try
            {
                await _notificadorPainel.NotificarNovoPedidoAsync(
                    n.VendedorId,
                    new NovoPedidoDto(salvo.Id, n.ProdutoId, n.NomeProduto, n.Quantidade, salvo.CreatedAt));
            }
            catch
            {
                // Best-effort: idem. A falha de um evento nao impede o outro.
            }
        }

        return new PedidoResultDto(true, "Pedido reservado com sucesso.", salvo.Id);

    }

    // ---------- Expiracao de reservas (P4/P5) ----------

    // Expira UMA reserva de forma atomica e idempotente: tenta marcar Expirado
    // (UPDATE condicional WHERE Status = Reservado). So quem "ganha" a corrida
    // (linha afetada) devolve o estoque -- garante que o estoque volta uma unica
    // vez, mesmo com o job (P4) e a checagem lazy (P5) concorrendo na mesma reserva.
    // Devolve true se ESTE chamador foi quem expirou.
    private async Task<bool> ExpirarReservaAsync(Pedido pedido)
    {
        if (!await _pedidoRepository.TryExpirarAsync(pedido.Id))
            return false; // outro caminho ja resolveu; nao devolve estoque de novo

        foreach (var item in pedido.Itens)
        {
            await _produtoRepository.IncrementarEstoqueAsync(item.ProdutoId, item.Quantidade);
        }

        return true;
    }

    // P4: usado pelo BackgroundService. Busca todas as reservas vencidas e expira
    // cada uma. Devolve quantas foram efetivamente expiradas por esta chamada.
    public async Task<int> ExpirarReservasVencidasAsync()
    {
        var vencidas = await _pedidoRepository.GetReservasVencidasAsync(DateTime.UtcNow);

        var expiradas = 0;
        foreach (var pedido in vencidas)
        {
            if (await ExpirarReservaAsync(pedido))
                expiradas++;
        }

        return expiradas;
    }

    // P5 (lazy): dada a pagina ja carregada de pedidos, expira SOMENTE as reservas
    // vencidas presentes nela (nao varre o banco). Rede de seguranca para quando o
    // job hibernou. Devolve os ids que foram expirados, para refletir no DTO.
    private async Task<HashSet<Guid>> ExpirarVencidasDaConsultaAsync(IEnumerable<Pedido> pedidos)
    {
        var agora = DateTime.UtcNow;
        var expirados = new HashSet<Guid>();

        foreach (var pedido in pedidos)
        {
            var venceu = pedido.Tipo == TipoPedido.Reserva
                && pedido.Status == PedidoStatus.Reservado
                && pedido.ExpiraEm is not null
                && pedido.ExpiraEm <= agora;

            if (venceu && await ExpirarReservaAsync(pedido))
                expirados.Add(pedido.Id);
        }

        return expirados;
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

        // P5: expira lazy as reservas vencidas desta pagina (rede de seguranca
        // caso o job tenha hibernado). O status expirado e refletido no DTO.
        var expirados = await ExpirarVencidasDaConsultaAsync(pedidos);

        var historico = pedidos
            .Select(p => new HistoricoPedidoDto(
                p.Id,
                expirados.Contains(p.Id) ? PedidoStatus.Expirado : p.Status,
                p.CreatedAt,
                p.Itens
                    .Select(i => new HistoricoItemPedidoDto(
                        i.ProdutoId,
                        i.Produto?.Nome ?? "Produto indisponivel",
                        i.Quantidade,
                        i.PrecoUnitarioNaCompra))
                    .ToList(),
                p.Itens.Sum(i => i.Quantidade * i.PrecoUnitarioNaCompra),
                p.Tipo,
                p.ExpiraEm))
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

    // P7: baixa manual do pedido pelo vendedor. usuarioId vem do token (claim "sub")
    // no Controller, nunca da requisicao.
    // Ordem: existe? -> o usuario e vendedor de algum item? -> a regra permite? ->
    // gravacao atomica (so se ainda estiver Reservado).
    public async Task<ConcluirPedidoResultDto> ConcluirPedidoAsync(Guid pedidoId, Guid usuarioId)
    {
        var pedido = await _pedidoRepository.GetByIdAsync(pedidoId);
        if (pedido is null)
        {
            return new ConcluirPedidoResultDto(ConcluirPedidoStatus.NaoEncontrado, "Pedido nao encontrado.");
        }

        // Anti-IDOR: so quem vende algum item do pedido pode conclui-lo.
        // A checagem vem antes da regra de estado para nao revelar o status a quem nao e dono.
        if (!await _pedidoRepository.PedidoTemItemDoVendedorAsync(pedidoId, usuarioId))
        {
            return new ConcluirPedidoResultDto(ConcluirPedidoStatus.Proibido, "Voce nao e vendedor de nenhum item deste pedido.");
        }

        // A regra de transicao fica na entidade (so Reservado pode ser concluido).
        // Aqui so validamos: nada e salvo por este objeto (a gravacao e o UPDATE abaixo).
        try
        {
            pedido.Concluir();
        }
        catch (InvalidOperationException ex)
        {
            return new ConcluirPedidoResultDto(ConcluirPedidoStatus.Conflito, ex.Message);
        }

        // UPDATE condicional (WHERE Status = Reservado): se o pedido mudou de estado
        // entre a leitura e a gravacao (ex.: expirou), nenhuma linha e afetada.
        if (!await _pedidoRepository.TryConcluirAsync(pedidoId))
        {
            return new ConcluirPedidoResultDto(ConcluirPedidoStatus.Conflito, "O pedido mudou de estado e nao pode mais ser concluido.");
        }

        return new ConcluirPedidoResultDto(ConcluirPedidoStatus.Sucesso, "Pedido concluido com sucesso.");
    }

    // P6: pedidos que contem produtos do vendedor autenticado (usuarioId vem do token).
    // O vendedor ve SO os itens dos proprios produtos e o total dessa parte; do comprador,
    // so o nome (LGPD). Quem nao tem perfil de vendedor recebe lista vazia.
    public async Task<(List<PedidoVendedorDto> Pedidos, int Total, int Page, int PageSize)> GetPedidosDoVendedorAsync(
        Guid usuarioId, int page, int pageSize)
    {
        // Mesma validacao de paginacao do historico do comprador.
        pageSize = pageSize < 1 ? PageSizePadrao : Math.Min(pageSize, PageSizeMaximo);
        page = Math.Clamp(page, 1, int.MaxValue / pageSize);

        var (pedidos, total) = await _pedidoRepository.GetByVendedorAsync(usuarioId, page, pageSize);

        // P5: expira lazy as reservas vencidas desta pagina (rede de seguranca
        // caso o job tenha hibernado). O status expirado e refletido no DTO.
        var expirados = await ExpirarVencidasDaConsultaAsync(pedidos);

        var lista = pedidos
            .Select(p =>
            {
                var itensDoVendedor = p.Itens
                    .Where(i => i.Produto?.Vendedor?.UsuarioId == usuarioId)
                    .Select(i => new PedidoVendedorItemDto(
                        i.ProdutoId,
                        i.Produto?.Nome ?? "Produto indisponivel",
                        i.Quantidade,
                        i.PrecoUnitarioNaCompra))
                    .ToList();

                return new PedidoVendedorDto(
                    p.Id,
                    expirados.Contains(p.Id) ? PedidoStatus.Expirado : p.Status,
                    p.Tipo,
                    p.CreatedAt,
                    p.ExpiraEm,
                    p.Comprador?.NomeCompleto ?? "Comprador",
                    itensDoVendedor,
                    itensDoVendedor.Sum(i => i.Quantidade * i.PrecoUnitarioNaCompra));
            })
            .ToList();

        return (lista, total, page, pageSize);
    }

    // CANCELAR: cancelamento de uma reserva pelo comprador. compradorId vem do token
    // (claim "sub") no Controller, nunca da requisicao.
    // Ordem: existe? -> o pedido e do usuario? -> a regra permite (so Reserva ainda
    // Reservada)? -> gravacao atomica -> devolve o estoque (so quem ganhou a corrida).
    public async Task<CancelarReservaResultDto> CancelarReservaAsync(Guid pedidoId, Guid compradorId)
    {
        var pedido = await _pedidoRepository.GetByIdAsync(pedidoId);
        if (pedido is null)
        {
            return new CancelarReservaResultDto(CancelarReservaStatus.NaoEncontrado, "Pedido nao encontrado.");
        }

        // Anti-IDOR: so o comprador pode cancelar o proprio pedido. A checagem vem
        // antes da regra de estado para nao revelar o status a quem nao e dono.
        if (pedido.CompradorId != compradorId)
        {
            return new CancelarReservaResultDto(CancelarReservaStatus.Proibido, "Este pedido nao e seu.");
        }

        // A regra de transicao fica na entidade (so Reserva ainda Reservada). Aqui so
        // validamos: nada e salvo por este objeto (a gravacao e o UPDATE abaixo).
        try
        {
            pedido.Cancelar();
        }
        catch (InvalidOperationException ex)
        {
            return new CancelarReservaResultDto(CancelarReservaStatus.Conflito, ex.Message);
        }

        // UPDATE condicional: se o pedido mudou de estado entre a leitura e a gravacao
        // (expirou, foi concluido ou ja foi cancelado por um duplo clique), nenhuma
        // linha e afetada e o estoque NAO e devolvido (ja foi, ou sera, pelo outro caminho).
        if (!await _pedidoRepository.TryCancelarAsync(pedidoId))
        {
            return new CancelarReservaResultDto(CancelarReservaStatus.Conflito, "A reserva mudou de estado e nao pode mais ser cancelada.");
        }

        // So quem ganhou a corrida chega aqui: o estoque volta uma unica vez.
        foreach (var item in pedido.Itens)
        {
            await _produtoRepository.IncrementarEstoqueAsync(item.ProdutoId, item.Quantidade);
        }

        return new CancelarReservaResultDto(CancelarReservaStatus.Sucesso, "Reserva cancelada com sucesso.");
    }
}
