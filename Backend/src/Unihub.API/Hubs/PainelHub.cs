using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UniHub.Application.Interfaces.Vendas;

namespace UniHub.API.Hubs;

// Hub do Painel do Vendedor em Tempo Real (Frente 2 / BRV-185).
//
// Responsabilidade (task A1): aceitar apenas conexoes autenticadas e agrupar
// cada conexao de um vendedor em "vendedor:{VendedorId}". Isso isola os eventos
// por dono -- um vendedor so recebe eventos dos seus proprios produtos/pedidos.
//
// Esta classe NAO emite eventos ainda; a emissao (EstoqueAtualizado/NovoPedido)
// chega nas tasks A3/A4 via a interface INotificadorPainel (Application) e a
// implementacao NotificadorPainelSignalR (API), usando IHubContext<PainelHub>.
// Por enquanto serve de stub para destravar o consumo no front (trilha B).
[Authorize]
public class PainelHub : Hub
{
    private readonly IVendedorConsultaRepository _vendedorConsulta;

    public PainelHub(IVendedorConsultaRepository vendedorConsulta)
    {
        _vendedorConsulta = vendedorConsulta;
    }

    // Prefixo dos grupos por vendedor. Exposto para A3/A4 montarem o mesmo nome
    // ao emitir eventos, evitando string solta repetida em pontos diferentes.
    public static string NomeGrupoVendedor(Guid vendedorId) => $"vendedor:{vendedorId}";

    public override async Task OnConnectedAsync()
    {
        // Guard defensivo: o [Authorize] ja barra quem nao tem token valido, e
        // todo token do projeto carrega "sub" (AuthService). Ainda assim, se o
        // claim vier ausente/invalido, abortamos em vez de seguir sem vendedor.
        // AGENTS Secao 4: SEMPRE via claim "sub" (MapInboundClaims = false),
        // NUNCA ClaimTypes.NameIdentifier (retornaria null neste projeto).
        var subClaim = Context.User?.FindFirst("sub")?.Value;
        if (!Guid.TryParse(subClaim, out var usuarioId))
        {
            Context.Abort();
            return;
        }

        // So vendedores entram em grupo. Clientes autenticados que nao sao
        // vendedores conectam normalmente, mas sem grupo -- logo nao recebem
        // nenhum evento do painel (nao ha broadcast global).
        var vendedorId = await _vendedorConsulta.ObterVendedorIdPorUsuarioAsync(usuarioId);
        if (vendedorId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, NomeGrupoVendedor(vendedorId.Value));
        }

        await base.OnConnectedAsync();
    }
}
