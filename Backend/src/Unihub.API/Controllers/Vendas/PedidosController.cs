using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.DTOs.Common;
using UniHub.Application.DTOs.Vendas;
using UniHub.Application.Services.Vendas;

namespace UniHub.API.Controllers.Vendas;

[ApiController]
[Route("api/pedidos")]
[Authorize]

public class PedidosController : ControllerBase
{
    private readonly PedidoService _pedidoService;

    public PedidosController(PedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    /*POST api/pedidos
    Endpoint alvo do teste de concorrencia: disparar N requisições
    Simultaneas com o mesmo ProdutoId e estoque baixo, e verificar
    que apenas as requisições cabíveis no estoque retornam sucesso.*/

    [HttpPost]
    public async Task<ActionResult<PedidoResultDto>> Create([FromBody] CreatePedidoDto dto)
    {
        var compradorId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var resultado = await _pedidoService.CreatePedidoAsync(compradorId, dto);

        if(!resultado.Sucesso)
            return Conflict(resultado); //Outro usuario levou o estoque primeiro
        
        return Ok(resultado);
    }

    // GET api/pedidos?page=1&pageSize=10
    // Historico de pedidos do usuario autenticado (BRV-89). O comprador vem SEMPRE
    // do token (claim "sub"), nunca da requisicao, para ninguem ver pedidos de outro.
    // O Service ajusta page/pageSize (padrao 10, maximo 50) e devolvemos os valores usados.
    // Reservas canceladas nao aparecem para o comprador.
    [HttpGet]
    public async Task<ActionResult<PaginatedResultDto<HistoricoPedidoDto>>> GetHistorico(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var compradorId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var (pedidos, total, paginaUsada, tamanhoUsado) =
            await _pedidoService.GetHistoricoAsync(compradorId, page, pageSize);

        var totalPaginas = (int)Math.Ceiling(total / (double)tamanhoUsado);

        return Ok(new PaginatedResultDto<HistoricoPedidoDto>(
            pedidos, paginaUsada, tamanhoUsado, total, totalPaginas));
    }

    // POST api/pedidos/{id}/concluir  (P7)
    // Baixa manual: o vendedor de algum item do pedido o marca como Concluido.
    // O vendedor vem SEMPRE do token (claim "sub"), nunca da requisicao.
    // 200 concluido | 401 token sem "sub" | 403 nao e vendedor do pedido |
    // 404 pedido inexistente | 409 pedido nao esta Reservado (ou mudou de estado).
    [HttpPost("{id:guid}/concluir")]
    public async Task<IActionResult> Concluir(Guid id)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var usuarioId))
            return Unauthorized();

        var resultado = await _pedidoService.ConcluirPedidoAsync(id, usuarioId);

        return resultado.Status switch
        {
            ConcluirPedidoStatus.Sucesso => Ok(resultado),
            ConcluirPedidoStatus.NaoEncontrado => NotFound(resultado),
            ConcluirPedidoStatus.Proibido => Forbid(),
            _ => Conflict(resultado)
        };
    }

    // POST api/pedidos/{id}/cancelar
    // O comprador cancela a PROPRIA reserva: o estoque volta e o pedido some do
    // historico dele (o vendedor continua vendo, marcado como Cancelado).
    // O comprador vem SEMPRE do token (claim "sub"), nunca da requisicao.
    // 200 cancelada | 401 token sem "sub" | 403 pedido de outro comprador |
    // 404 pedido inexistente | 409 nao e Reserva, ou nao esta mais Reservada.
    [HttpPost("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var compradorId))
            return Unauthorized();

        var resultado = await _pedidoService.CancelarReservaAsync(id, compradorId);

        return resultado.Status switch
        {
            CancelarReservaStatus.Sucesso => Ok(resultado),
            CancelarReservaStatus.NaoEncontrado => NotFound(resultado),
            CancelarReservaStatus.Proibido => Forbid(),
            _ => Conflict(resultado)
        };
    }

    // GET api/pedidos/vendedor?page=1&pageSize=10  (P6)
    // Pedidos que contem produtos do vendedor autenticado. O vendedor vem SEMPRE do
    // token (claim "sub"), nunca da query. Quem nao e vendedor recebe lista vazia.
    // Do comprador, so o nome (LGPD). Mesmo contrato paginado do historico.
    [HttpGet("vendedor")]
    public async Task<ActionResult<PaginatedResultDto<PedidoVendedorDto>>> GetPedidosDoVendedor(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var usuarioId))
            return Unauthorized();

        var (pedidos, total, paginaUsada, tamanhoUsado) =
            await _pedidoService.GetPedidosDoVendedorAsync(usuarioId, page, pageSize);

        var totalPaginas = (int)Math.Ceiling(total / (double)tamanhoUsado);

        return Ok(new PaginatedResultDto<PedidoVendedorDto>(
            pedidos, paginaUsada, tamanhoUsado, total, totalPaginas));
    }
}