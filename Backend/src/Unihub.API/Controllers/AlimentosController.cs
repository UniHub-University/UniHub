using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.DTOs.Alimentacao;
using UniHub.Application.Services.Alimentacao;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/alimentos")]
[Authorize]
public class AlimentosController : ControllerBase
{
    private readonly AlimentoService _alimentoService;

    public AlimentosController(AlimentoService alimentoService)
    {
        _alimentoService = alimentoService;
    }

    // GET /api/alimentos
    // Retorna alimentos disponíveis para os usuários
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AlimentoResumoDto>>> ObterDisponiveis()
    {
        var alimentos = await _alimentoService.ObterDisponiveisAsync();

        return Ok(alimentos);
    }

    // GET /api/alimentos/{id}
    // Permite consultar também um alimento esgotado
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AlimentoRespostaDto>> ObterPorId(Guid id)
    {
        var alimento = await _alimentoService.ObterPorIdAsync(id);

        if (alimento is null)
        {
            return NotFound(new
            {
                mensagem = "Alimento não encontrado."
            });
        }

        return Ok(alimento);
    }

    // POST /api/alimentos
    [HttpPost]
    public async Task<ActionResult<AlimentoRespostaDto>> Criar(
        [FromBody] CriarAlimentoDto dto)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
        {
            return Unauthorized(new
            {
                mensagem = "Usuário autenticado não identificado."
            });
        }

        try
        {
            var alimento = await _alimentoService.CriarAsync(
                usuarioId,
                dto);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = alimento.Id },
                alimento);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensagem = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                mensagem = ex.Message
            });
        }
    }

    // PUT /api/alimentos/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(
        Guid id,
        [FromBody] AtualizarAlimentoDto dto)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
        {
            return Unauthorized(new
            {
                mensagem = "Usuário autenticado não identificado."
            });
        }

        try
        {
            var atualizado = await _alimentoService.AtualizarAsync(
                usuarioId,
                id,
                dto);

            if (!atualizado)
            {
                return NotFound(new
                {
                    mensagem = "Alimento não encontrado ou não pertence ao vendedor."
                });
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                mensagem = ex.Message
            });
        }
    }

    // DELETE /api/alimentos/{id}
    // Realiza inativação lógica, não exclusão física
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Inativar(Guid id)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
        {
            return Unauthorized(new
            {
                mensagem = "Usuário autenticado não identificado."
            });
        }

        var inativado = await _alimentoService.InativarAsync(
            usuarioId,
            id);

        if (!inativado)
        {
            return NotFound(new
            {
                mensagem = "Alimento não encontrado ou não pertence ao vendedor."
            });
        }

        return NoContent();
    }

    // PATCH /api/alimentos/{id}/reativar
    [HttpPatch("{id:guid}/reativar")]
    public async Task<IActionResult> Reativar(Guid id)
    {
        if (!TentarObterUsuarioId(out var usuarioId))
        {
            return Unauthorized(new
            {
                mensagem = "Usuário autenticado não identificado."
            });
        }

        var reativado = await _alimentoService.ReativarAsync(
            usuarioId,
            id);

        if (!reativado)
        {
            return BadRequest(new
            {
                mensagem =
                    "Não foi possível reativar o alimento. " +
                    "Verifique se ele pertence ao vendedor e possui estoque."
            });
        }

        return NoContent();
    }

    private bool TentarObterUsuarioId(out Guid usuarioId)
    {
        var valorClaim = User.FindFirst("sub")?.Value;

        return Guid.TryParse(valorClaim, out usuarioId);
    }
}
