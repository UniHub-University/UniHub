using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs.AcaoSolidaria;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SolicitacaoApoioController : ControllerBase
{
    private readonly AppDbContext _context;

    public SolicitacaoApoioController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Cria uma nova solicitação vinculada ao usuário autenticado.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarSolicitacaoApoioDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Descricao) || string.IsNullOrWhiteSpace(dto.Categoria))
            return BadRequest(new { mensagem = "Categoria e descrição são obrigatórias." });

        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized(new { mensagem = "Usuário não autenticado ou token inválido." });

        var solicitacao = new SolicitacaoApoio
        {
            SolicitanteId = usuarioId,
            Categoria = dto.Categoria.Trim(),
            Descricao = dto.Descricao.Trim()
        };

        _context.SolicitacoesApoio.Add(solicitacao);
        await _context.SaveChangesAsync();

        var resposta = new SolicitacaoApoioRespostaDto(
            solicitacao.Id,
            solicitacao.CodigoPublico,
            solicitacao.Categoria,
            solicitacao.Descricao,
            solicitacao.Status,
            solicitacao.CreatedAt
        );

        return CreatedAtAction(nameof(ObterPorId), new { id = solicitacao.Id }, resposta);
    }

    /// <summary>
    /// Lista pública de solicitações abertas. Exibe apenas o código público e nunca a identidade.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ObterAbertas()
    {
        // Regra LGPD: Sem .Include(s => s.Solicitante)
        var solicitacoes = await _context.SolicitacoesApoio
            .AsNoTracking()
            .Where(s => s.Status == StatusSolicitacao.Aberta)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new SolicitacaoApoioRespostaDto(
                s.Id,
                s.CodigoPublico,
                s.Categoria,
                s.Descricao,
                s.Status,
                s.CreatedAt
            ))
            .ToListAsync();

        return Ok(solicitacoes);
    }

    /// <summary>
    /// Obtém os detalhes públicos de uma solicitação pelo ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var solicitacao = await _context.SolicitacoesApoio
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SolicitacaoApoioRespostaDto(
                s.Id,
                s.CodigoPublico,
                s.Categoria,
                s.Descricao,
                s.Status,
                s.CreatedAt
            ))
            .FirstOrDefaultAsync();

        if (solicitacao == null)
            return NotFound(new { mensagem = "Solicitação não encontrada." });

        return Ok(solicitacao);
    }

    /// <summary>
    /// Cancela a solicitação. Apenas o próprio autor autenticado pode cancelar.
    /// </summary>
    [HttpPatch("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id)
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim) || !Guid.TryParse(usuarioIdClaim, out var usuarioId))
            return Unauthorized();

        var solicitacao = await _context.SolicitacoesApoio.FirstOrDefaultAsync(s => s.Id == id);
        if (solicitacao == null)
            return NotFound(new { mensagem = "Solicitação não encontrada." });

        // Validação de posse
        if (solicitacao.SolicitanteId != usuarioId)
            return Forbid();

        try
        {
            solicitacao.Cancelar();
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}