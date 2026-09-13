using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs.Bazar;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/itens-bazar")]
public class ItensBazarController : ControllerBase
{
    private readonly AppDbContext _context;

    public ItensBazarController(AppDbContext context)
    {
        _context = context;
    }

    // CREATE: POST /api/itens-bazar
    [HttpPost]
    public async Task<ActionResult<ItemBazarRespostaDto>> Criar([FromBody] CriarItemBazarDto dto)
    {
        var vendedorExiste = await _context.Vendedores.AnyAsync(v => v.Id == dto.VendedorId);
        if (!vendedorExiste)
        {
            return BadRequest(new { mensagem = "Vendedor não encontrado com o ID informado." });
        }

        var item = new ItemBazar
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            Preco = dto.Preco,
            QuantidadeDisponivel = dto.QuantidadeDisponivel,
            Categoria = dto.Categoria,
            Condicao = dto.Condicao,
            VendedorId = dto.VendedorId,
            DataPublicacao = DateTime.UtcNow,
            Status = ProdutoStatus.Disponivel
        };

        _context.ItensBazar.Add(item);
        await _context.SaveChangesAsync();

        var resposta = MapearParaDto(item);
        return CreatedAtAction(nameof(ObterPorId), new { id = item.Id }, resposta);
    }

    // READ: GET /api/itens-bazar
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemBazarRespostaDto>>> ObterTodos()
    {
        var itens = await _context.ItensBazar
            .AsNoTracking()
            .Select(item => MapearParaDto(item))
            .ToListAsync();

        return Ok(itens);
    }

    // READ: GET /api/itens-bazar/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemBazarRespostaDto>> ObterPorId(Guid id)
    {
        var item = await _context.ItensBazar.FindAsync(id);

        if (item == null)
            return NotFound(new { mensagem = "Item do bazar não encontrado." });

        return Ok(MapearParaDto(item));
    }

    // UPDATE: PUT /api/itens-bazar/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarItemBazarDto dto)
    {
        var item = await _context.ItensBazar.FindAsync(id);

        if (item == null)
            return NotFound(new { mensagem = "Item do bazar não encontrado." });

        item.Nome = dto.Nome;
        item.Descricao = dto.Descricao;
        item.Preco = dto.Preco;
        item.QuantidadeDisponivel = dto.QuantidadeDisponivel;
        item.Categoria = dto.Categoria;
        item.Condicao = dto.Condicao;
        item.Status = dto.Status;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: DELETE /api/itens-bazar/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id)
    {
        var item = await _context.ItensBazar.FindAsync(id);

        if (item == null)
            return NotFound(new { mensagem = "Item do bazar não encontrado." });

        _context.ItensBazar.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private static ItemBazarRespostaDto MapearParaDto(ItemBazar item) =>
        new(
            item.Id,
            item.Nome,
            item.Descricao,
            item.Preco,
            item.QuantidadeDisponivel,
            item.Categoria,
            item.Condicao,
            item.Status,
            item.DataPublicacao,
            item.VendedorId
        );
}