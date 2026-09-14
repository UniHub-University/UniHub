using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}