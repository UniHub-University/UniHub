// Backend/src/Unihub.API/Controllers/DoacaoController.cs
using System.Linq.Expressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs.Solidariedade;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/doacoes")]
[Authorize]
public class DoacaoController : ControllerBase
{
    private readonly AppDbContext _context;

    public DoacaoController(AppDbContext context)
    {
        _context = context;
    }

    // CREATE: POST /api/doacoes
    // Voluntário registra uma doação para uma solicitação que ele já aceitou.
    [HttpPost]
    public async Task<ActionResult<DoacaoRespostaDto>> Criar([FromBody] CriarDoacaoDto dto)
    {
        var voluntario = await ObterVoluntarioLogado();
        if (voluntario is null)
            return BadRequest(new { mensagem = "Usuario precisa ser voluntario para registrar doacoes." });

        // Só pode doar quem tem uma correspondência ACEITA para essa solicitação.
        var temAceite = await _context.Correspondencias.AnyAsync(c =>
            c.SolicitacaoId == dto.SolicitacaoId &&
            c.VoluntarioId == voluntario.Id &&
            c.Status == StatusCorrespondencia.AceitaPeloVoluntario);

        if (!temAceite)
            return Conflict(new { mensagem = "Voce precisa ter aceitado a solicitacao antes de registrar uma doacao." });

        var doacao = new Doacao
        {
            SolicitacaoId = dto.SolicitacaoId,
            VoluntarioId = voluntario.Id,
            Descricao = dto.Descricao
        };

        _context.Doacoes.Add(doacao);
        await _context.SaveChangesAsync();

        var resposta = await ProjetarAsync(d => d.Id == doacao.Id);
        return CreatedAtAction(nameof(ObterPorId), new { id = doacao.Id }, resposta.Single());
    }

    // READ: GET /api/doacoes/minhas
    // PRIVACIDADE: projeta direto para o DTO, sem Include(s => s.Solicitante).
    [HttpGet("minhas")]
    public async Task<ActionResult<IEnumerable<DoacaoRespostaDto>>> ListarMinhas()
    {
        var voluntario = await ObterVoluntarioLogado();
        if (voluntario is null)
            return BadRequest(new { mensagem = "Usuario precisa ser voluntario para listar doacoes." });

        return Ok(await ProjetarAsync(d => d.VoluntarioId == voluntario.Id));
    }

    // READ: GET /api/doacoes/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DoacaoRespostaDto>> ObterPorId(Guid id)
    {
        var voluntario = await ObterVoluntarioLogado();
        if (voluntario is null)
            return Forbid();

        var lista = await ProjetarAsync(d => d.Id == id && d.VoluntarioId == voluntario.Id);
        if (lista.Count == 0)
            return NotFound(new { mensagem = "Doacao nao encontrada." });

        return Ok(lista[0]);
    }

    // POST /api/doacoes/{id}/confirmar-entrega
    [HttpPost("{id:guid}/confirmar-entrega")]
    public Task<IActionResult> ConfirmarEntrega(Guid id)
        => ExecutarAcao(id, d => d.ConfirmarEntrega());

    // POST /api/doacoes/{id}/cancelar
    [HttpPost("{id:guid}/cancelar")]
    public Task<IActionResult> Cancelar(Guid id)
        => ExecutarAcao(id, d => d.Cancelar());

    // ---------- helpers ----------

    private async Task<IActionResult> ExecutarAcao(Guid id, Action<Doacao> acao)
    {
        var doacao = await _context.Doacoes.FindAsync(id);
        if (doacao is null)
            return NotFound(new { mensagem = "Doacao nao encontrada." });

        var voluntario = await ObterVoluntarioLogado();
        if (voluntario is null || doacao.VoluntarioId != voluntario.Id)
            return Forbid(); // 403: a doacao existe, mas nao pertence a esse usuario

        try
        {
            acao(doacao); // a regra de estado vive na entidade
            await _context.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }

        return NoContent();
    }

    private async Task<List<DoacaoRespostaDto>> ProjetarAsync(Expression<Func<Doacao, bool>> filtro)
        => await _context.Doacoes
            .AsNoTracking()
            .Where(filtro)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DoacaoRespostaDto(
                d.Id,
                d.Solicitacao!.CodigoPublico, // só o pseudônimo, nunca o Solicitante
                d.Descricao,
                d.Status,
                d.CreatedAt,
                d.EntregueEm))
            .ToListAsync();

    private async Task<Voluntario?> ObterVoluntarioLogado()
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        // AJUSTAR: confirme se Voluntario tem a FK UsuarioId (igual a Vendedor)
        return await _context.Voluntarios.AsNoTracking().FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
    }
}
