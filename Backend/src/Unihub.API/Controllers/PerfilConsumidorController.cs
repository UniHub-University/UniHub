using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/perfil/consumidor")]
[Authorize]
public class PerfilConsumidorController : ControllerBase
{
    private readonly AppDbContext _context;

    public PerfilConsumidorController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Atualiza as restrições alimentares do consumidor autenticado.
    /// </summary>
    [HttpPut("restricoes")]
    public async Task<IActionResult> AtualizarRestricoes([FromBody] AtualizarRestricoesDto dto)
    {
        // 1. Extração do identificador do utilizador autenticado a partir do token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("sub")?.Value 
                       ?? User.FindFirst("id")?.Value;

        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value 
                      ?? User.FindFirst("email")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) && string.IsNullOrEmpty(emailClaim))
        {
            return Unauthorized(new { mensagem = "Token inválido: nenhuma identificação encontrada nas claims." });
        }

        bool isGuid = Guid.TryParse(userIdClaim, out var guidIdentificador);

        // 2. Busca simplificada do utilizador autenticado
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => 
            (isGuid && u.Id == guidIdentificador) ||
            (!string.IsNullOrEmpty(emailClaim) && u.EmailInstitucional == emailClaim)
        );

        if (usuario == null)
        {
            return NotFound(new { mensagem = "Utilizador autenticado não encontrado na base de dados." });
        }

        var idsDesejados = dto?.RestricoesIds ?? new List<Guid>();

        // 3. Acesso direto via DbSet adicionado no AppDbContext (_context.RestricaoAlimentar)
        var restricoesAtuais = await _context.RestricoesAlimentares
            .Include(r => r.Usuarios)
            .Where(r => r.Usuarios.Any(u => u.Id == usuario.Id))
            .ToListAsync();

        // 4. Validação da existência dos IDs enviados
        List<RestricaoAlimentar> novasRestricoes = new();
        if (idsDesejados.Any())
        {
            novasRestricoes = await _context.RestricoesAlimentares
                .Include(r => r.Usuarios)
                .Where(r => idsDesejados.Contains(r.Id))
                .ToListAsync();

            if (novasRestricoes.Count != idsDesejados.Distinct().Count())
            {
                return BadRequest(new { mensagem = "Um ou mais IDs de restrições alimentares informados são inválidos ou não existem." });
            }
        }

        // 5. Remove o vínculo com as opções desmarcadas
        foreach (var restricao in restricoesAtuais)
        {
            if (!idsDesejados.Contains(restricao.Id))
            {
                restricao.Usuarios.Remove(usuario);
            }
        }

        // 6. Adiciona o vínculo com as novas opções selecionadas
        foreach (var restricao in novasRestricoes)
        {
            if (!restricao.Usuarios.Any(u => u.Id == usuario.Id))
            {
                restricao.Usuarios.Add(usuario);
            }
        }

        // 7. Salva as alterações na base de dados
        await _context.SaveChangesAsync();

        return Ok("Restrições alimentares atualizadas com sucesso.");
    }
}