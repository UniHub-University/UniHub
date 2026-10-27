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
}