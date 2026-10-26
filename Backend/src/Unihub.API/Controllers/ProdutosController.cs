using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.DTOs.Common;
using UniHub.Application.DTOs.Produtos;
using UniHub.Application.Services.Vendas;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/produtos")]
[Authorize]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoService _produtoService;

    public ProdutosController(ProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    // GET /api/produtos?termo=&tipo=bazar&precoMin=&precoMax=&campus=&diaSemana=&hora=12:00&page=1&pageSize=12
    [HttpGet]
    public async Task<ActionResult<PaginatedResultDto<ProdutoResumoDto>>> Buscar([FromQuery] BuscaProdutosQueryDto query)
    {
        try
        {
            var resultado = await _produtoService.BuscarAsync(query);
            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
