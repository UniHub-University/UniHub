using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.Services.Vendas;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")] // Protege o controller
public class AdminController : ControllerBase
{
    private readonly ProdutoService _produtoService;

    public AdminController(ProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpDelete("produtos/{id:guid}/inativar")]
    public async Task<IActionResult> InativarProduto(Guid id)
    {
        var sucesso = await _produtoService.InativarProdutoAsync(id);

        if (!sucesso)
        {
            return NotFound(new { mensagem = "Produto não encontrado no sistema." });
        }

        return NoContent(); // Retorna HTTP 204 (No Content)
    }
}