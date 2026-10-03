using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.Interfaces.Vendas;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")] // Protege o controller
public class AdminController : ControllerBase
{
    private readonly IProdutoRepository _produtoRepository;

    public AdminController(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    [HttpDelete("produtos/{id:guid}/inativar")]
    public async Task<IActionResult> InativarProduto(Guid id)
    {
        var sucesso = await _produtoRepository.InativarAsync(id);

        if (!sucesso)
        {
            return NotFound(new { mensagem = "Produto não encontrado no sistema." });
        }

        return NoContent(); // Retorna HTTP 204 (No Content)
    }
}