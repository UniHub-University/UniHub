using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.Services.Vendas;
using UniHub.Application.Interfaces;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")] // Protege o controller
public class AdminController : ControllerBase
{
    private readonly ProdutoService _produtoService;
    private readonly IUsuarioRepository _usuarioRepository;

    public AdminController(ProdutoService produtoService, IUsuarioRepository usuarioRepository)
    {
        _produtoService = produtoService;
        _usuarioRepository = usuarioRepository;
    }

    [HttpPatch("produtos/{id:guid}/inativar")]
    public async Task<IActionResult> InativarProduto(Guid id)
    {
        var sucesso = await _produtoService.InativarProdutoAsync(id);

        if (!sucesso)
        {
            return NotFound(new { mensagem = "Produto não encontrado no sistema." });
        }

        return NoContent(); // Retorna HTTP 204 (No Content)
    }

    [HttpPatch("usuarios/{id:guid}/suspender")]
    public async Task<IActionResult> SuspenderConta(Guid id)
    {

        // Impede que um administrador suspenda a própria conta
        var adminId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        
        if (adminId == id)
            return BadRequest(new { mensagem = "Um administrador não pode suspender a própria conta." });

        var sucesso = await _usuarioRepository.SuspenderAsync(id);
        
        if (!sucesso)
        {
            return NotFound(new { mensagem = "Usuário não encontrado no sistema." });
        }

        return NoContent(); // Retorna HTTP 204 (No Content)
    }

    [HttpPatch("usuarios/{id:guid}/reativar")]
    public async Task<IActionResult> ReativarConta(Guid id)
    {
        var sucesso = await _usuarioRepository.ReativarAsync(id);

        if (!sucesso)
        {
            return NotFound(new {mensagem = "Usuário não encontrado no sistema."});
        }

        return NoContent(); // Se for sucesso, retona 204
    }
}