using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs.AcaoSolidaria;
using UniHub.Application.Interfaces.AcaoSolidaria;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/correspondencias")]
[Authorize]
public class CorrespondenciaController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ISolicitacaoApoioRepository _solicitacoes;

    public CorrespondenciaController(AppDbContext context, ISolicitacaoApoioRepository solicitacoes)
    {
        _context = context;
        _solicitacoes = solicitacoes;
    }

    // GET /api/correspondencias/sugestoes
    [HttpGet("sugestoes")]
    public async Task<ActionResult<IEnumerable<SolicitacaoApoioRespostaDto>>> Sugestoes()
    {
        var voluntario = await ObterVoluntarioLogado();
        if (voluntario is null)
            return BadRequest(new { mensagem = "Usuario precisa ser voluntario para ver sugestoes." });

        var areas = voluntario.AreasDeAtuacao;

        var sugestoes = await _context.SolicitacoesApoio
            .AsNoTracking()
            .Where(s => s.Status == StatusSolicitacao.Aberta
                     && s.SolicitanteId != voluntario.UsuarioId
                     && areas.Contains(s.Categoria))
            .OrderBy(s => s.CreatedAt)
            .Select(s => new SolicitacaoApoioRespostaDto(
                s.Id, s.CodigoPublico, s.Categoria, s.Descricao, s.Status, s.CreatedAt))
            .ToListAsync();

        return Ok(sugestoes);
    }

    // POST /api/correspondencias/aceitar
    [HttpPost("aceitar")]
    public async Task<ActionResult<CorrespondenciaRespostaDto>> Aceitar([FromBody] AceitarSolicitacaoDto dto)
    {
        if (dto.SolicitacaoId == Guid.Empty)
            return BadRequest(new { mensagem = "O ID da solicitação é obrigatório." });

        var voluntario = await ObterVoluntarioLogado();
        if (voluntario is null)
            return BadRequest(new { mensagem = "Usuario precisa ser voluntario para aceitar solicitacoes." });

        var solicitacao = await _context.SolicitacoesApoio.FindAsync(dto.SolicitacaoId);
        if (solicitacao is null)
            return NotFound(new { mensagem = "Solicitacao nao encontrada." });

        if (solicitacao.SolicitanteId == voluntario.UsuarioId)
            return BadRequest(new { mensagem = "Voce nao pode atender a propria solicitacao." });

        // a trava: so deixa passar quem chegar primeiro, mesmo com dois clicando ao mesmo tempo
        await using var transacao = await _context.Database.BeginTransactionAsync();

        var venceu = await _solicitacoes.TryMarcarEmAndamentoAsync(dto.SolicitacaoId);
        if (!venceu)
            return Conflict(new { mensagem = "Esta solicitacao nao esta mais disponivel." });

        try
        {
            var correspondencia = new Correspondencia
            {
                SolicitacaoId = dto.SolicitacaoId,
                VoluntarioId = voluntario.Id
            };
            correspondencia.Aceitar();

            _context.Correspondencias.Add(correspondencia);
            await _context.SaveChangesAsync();
            await transacao.CommitAsync();

            return Ok(new CorrespondenciaRespostaDto(
                correspondencia.Id,
                solicitacao.CodigoPublico,
                solicitacao.Categoria,
                correspondencia.Status,
                correspondencia.CreatedAt,
                correspondencia.AceitaEm,
                correspondencia.IdentidadeRevelada));
        }
        catch (InvalidOperationException ex)
        {
            // transacao sofre rollback automatico pelo 'await using' ao sair sem commit
            return Conflict(new { mensagem = ex.Message });
        }
    }

    // POST /api/correspondencias/{id}/concluir
    [HttpPost("{id:guid}/concluir")]
    public Task<IActionResult> Concluir(Guid id) => ExecutarAcao(id, c => c.Concluir());

    // POST /api/correspondencias/{id}/cancelar
    [HttpPost("{id:guid}/cancelar")]
    public Task<IActionResult> Cancelar(Guid id) => ExecutarAcao(id, c => c.Cancelar());

    // POST /api/correspondencias/{id}/revelar-identidade
    // so o solicitante pode chamar, nunca acontece sozinho
    [HttpPost("{id:guid}/revelar-identidade")]
    public async Task<IActionResult> RevelarIdentidade(Guid id)
    {
        var correspondencia = await _context.Correspondencias
            .Include(c => c.Solicitacao)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (correspondencia is null)
            return NotFound(new { mensagem = "Correspondencia nao encontrada." });

        var usuarioId = ObterUsuarioIdLogado();
        if (correspondencia.Solicitacao?.SolicitanteId != usuarioId)
            return Forbid();

        try
        {
            correspondencia.AutorizarRevelacaoIdentidade();
            await _context.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }

        return NoContent();
    }

    // ---------- helpers ----------

    private async Task<IActionResult> ExecutarAcao(Guid id, Action<Correspondencia> acao)
    {
        var correspondencia = await _context.Correspondencias
            .Include(c => c.Voluntario)
            .Include(c => c.Solicitacao)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (correspondencia is null)
            return NotFound(new { mensagem = "Correspondencia nao encontrada." });

        var usuarioId = ObterUsuarioIdLogado();
        var pertenceAoVoluntario = correspondencia.Voluntario?.UsuarioId == usuarioId;
        var pertenceAoSolicitante = correspondencia.Solicitacao?.SolicitanteId == usuarioId;

        if (!pertenceAoVoluntario && !pertenceAoSolicitante)
            return Forbid();

        try
        {
            acao(correspondencia);
            await _context.SaveChangesAsync();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }

        return NoContent();
    }

    private Guid ObterUsuarioIdLogado() =>
        Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());

    private async Task<Voluntario?> ObterVoluntarioLogado()
    {
        var usuarioId = ObterUsuarioIdLogado();
        return await _context.Voluntarios.AsNoTracking().FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
    }
}