using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.Services.Vendas;
using UniHub.Application.Interfaces;
using UniHub.Application.DTOs.Denuncias;
using UniHub.Domain.Entities;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")] // Protege o controller
public class AdminController : ControllerBase
{
    private readonly ProdutoService _produtoService;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IDenunciaRepository _denunciaRepository;

    public AdminController(ProdutoService produtoService, IUsuarioRepository usuarioRepository, IDenunciaRepository denunciaRepository)
    {
        _produtoService = produtoService;
        _usuarioRepository = usuarioRepository;
        _denunciaRepository = denunciaRepository;
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

    // PATCH /api/admin/denuncias/{id}/status
    // Atualiza o status de uma denúncia conforme as regras de transição do domínio.
    // Respostas: 204 sucesso, 404 não encontrada, 400 status inválido, 409 transição recusada.
    [HttpPatch("denuncias/{id:guid}/status")]
    public async Task<IActionResult> AtualizarStatusDenuncia(Guid id, [FromBody] AtualizarStatusDenunciaDto dto)
    {
        // Valida o enum antes de tocar na entidade (evita 500 por valor fora da faixa).
        if (!Enum.IsDefined(typeof(DenunciaStatus), dto.Status))
            return BadRequest(new { mensagem = "Status inválido. Use: 0 Aberta, 1 EmAnalise, 2 Resolvida, 3 Rejeitada." });

        var denuncia = await _denunciaRepository.ObterRastreadaAsync(id);
        if (denuncia is null)
            return NotFound(new { mensagem = "Denúncia não encontrada." });

        try
        {
            denuncia.AtualizarStatus(dto.Status);
            await _denunciaRepository.SalvarAlteracoesAsync();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            // Transição recusada (ex.: estado final não muda).
            return Conflict(new { mensagem = ex.Message });
        }
    }
}