using Microsoft.AspNetCore.SignalR;
using UniHub.Application.DTOs.Vendas;
using UniHub.Application.Interfaces.Vendas;

namespace UniHub.API.Hubs;

// Implementacao (na camada API) do INotificadorPainel usando SignalR.
// Envia cada evento APENAS ao grupo do vendedor dono ("vendedor:{id}"),
// garantindo o isolamento por vendedor (AGENTS: um vendedor nao recebe eventos
// de outro). Reusa PainelHub.NomeGrupoVendedor para montar o mesmo nome de
// grupo usado no OnConnectedAsync, evitando string magica divergente.
//
// Os nomes dos eventos ("EstoqueAtualizado"/"NovoPedido") sao o contrato
// acordado com o front (trilha B). O SignalR serializa o payload em camelCase.
public class NotificadorPainelSignalR : INotificadorPainel
{
    private readonly IHubContext<PainelHub> _hub;

    public NotificadorPainelSignalR(IHubContext<PainelHub> hub)
    {
        _hub = hub;
    }

    public Task NotificarEstoqueAsync(Guid vendedorId, EstoqueAtualizadoDto evento) =>
        _hub.Clients
            .Group(PainelHub.NomeGrupoVendedor(vendedorId))
            .SendAsync("EstoqueAtualizado", evento);

    public Task NotificarNovoPedidoAsync(Guid vendedorId, NovoPedidoDto evento) =>
        _hub.Clients
            .Group(PainelHub.NomeGrupoVendedor(vendedorId))
            .SendAsync("NovoPedido", evento);
}
