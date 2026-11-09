using UniHub.Application.DTOs.Vendas;

namespace UniHub.Application.Interfaces.Vendas;

// Abstracao (na Application) para notificar o Painel do Vendedor em tempo real,
// sem acoplar a camada de regra de negocio ao SignalR. A implementacao
// (NotificadorPainelSignalR, na API) usa IHubContext<PainelHub> e envia ao
// grupo "vendedor:{id}". Assim o PedidoService (A4) depende so desta interface,
// preservando a direcao de dependencia da Clean Architecture (AGENTS Secao 3):
// Application NAO conhece a API.
//
// O vendedorId e passado por quem emite (o PedidoService ja tem o
// produto.VendedorId), entao a implementacao so traduz para o grupo do hub.
public interface INotificadorPainel
{
    // Notifica que o estoque de um produto do vendedor mudou (pos-decremento).
    Task NotificarEstoqueAsync(Guid vendedorId, EstoqueAtualizadoDto evento);

    // Notifica que um novo pedido afetou um produto do vendedor.
    Task NotificarNovoPedidoAsync(Guid vendedorId, NovoPedidoDto evento);
}
