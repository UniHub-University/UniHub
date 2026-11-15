using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs.Bazar;
using UniHub.Application.Interfaces;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/itens-bazar")]
[Authorize]
public class ItensBazarController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IImageStorageService _imageStorage;

    public ItensBazarController(AppDbContext context, IImageStorageService imageStorage)
    {
        _context = context;
        _imageStorage = imageStorage;
    }

        // CREATE: POST /api/itens-bazar
    [HttpPost]
    public async Task<ActionResult<ItemBazarRespostaDto>> Criar([FromBody] CriarItemBazarDto dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
        if (vendedor is null)
            return BadRequest(new { mensagem = "Usuario precisa ter um perfil de vendedor para publicar itens." });

        var item = new ItemBazar
        {
            Nome = dto.Nome,
            Descricao = dto.Descricao,
            ImagemUrl = string.IsNullOrWhiteSpace(dto.ImagemUrl) ? null : dto.ImagemUrl,
            Preco = dto.Preco,
            QuantidadeDisponivel = dto.QuantidadeDisponivel,
            Categoria = dto.Categoria,
            Condicao = dto.Condicao,
            VendedorId = vendedor.Id, // nao vem mais do dto
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
    public async Task<ActionResult<IEnumerable<ItemBazarResumoDto>>> ObterTodos()
    {
        var itens = await _context.ItensBazar.AsNoTracking().ToListAsync();
        var resposta = itens.Select(MapearParaResumoDto);
        return Ok(resposta);
    }

    // READ: GET /api/itens-bazar/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ItemBazarRespostaDto>> ObterPorId(Guid id)
    {
        var item = await _context.ItensBazar.Include(i => i.Vendedor).FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
            return NotFound(new { mensagem = "Item do bazar não encontrado." });

        return Ok(MapearParaDto(item));
    }


    // Consulta exclusiva do editor: autenticação não equivale a posse do produto.
    [HttpGet("{id:guid}/edicao")]
    public async Task<ActionResult<ItemBazarRespostaDto>> ObterParaEdicao(Guid id)
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value, out var usuarioId))
            return Unauthorized();

        var item = await _context.ItensBazar.AsNoTracking()
            .Include(i => i.Vendedor).FirstOrDefaultAsync(i => i.Id == id);
        if (item is null)
            return NotFound(new { mensagem = "Item do bazar não encontrado." });

        if (item.Vendedor is null || item.Vendedor.UsuarioId != usuarioId)
            return Forbid();

        return Ok(MapearParaDto(item));
    }

       // UPDATE: PUT /api/itens-bazar/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarItemBazarDto dto)
    {
        var item = await _context.ItensBazar.FindAsync(id);

        if (item == null)
            return NotFound(new { mensagem = "Item do bazar não encontrado." });

        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
        if (vendedor is null || item.VendedorId != vendedor.Id)
            return Forbid(); // 403: item existe, mas nao pertence a esse usuario

        var novaImagemUrl = string.IsNullOrWhiteSpace(dto.ImagemUrl) ? null : dto.ImagemUrl;
        // Se a imagem mudou (troca ou remoção), apaga a antiga do Cloudinary.
        if (item.ImagemUrl != novaImagemUrl)
        {
            await RemoverImagemAntigaAsync(item.ImagemUrl);
        }

        item.Nome = dto.Nome;
        item.Descricao = dto.Descricao;
        item.ImagemUrl = novaImagemUrl;
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

        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
        if (vendedor is null || item.VendedorId != vendedor.Id)
            return Forbid(); // 403: item existe, mas nao pertence a esse usuario

        _context.ItensBazar.Remove(item);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    private static ItemBazarResumoDto MapearParaResumoDto(ItemBazar item) =>
        new(
            item.Id, 
            item.Nome, 
            item.Descricao, 
            item.ImagemUrl,
            item.Preco,
            item.QuantidadeDisponivel, 
            item.Categoria, 
            item.Condicao,
            item.Status, 
            item.DataPublicacao, 
            item.VendedorId
    );

    private static ItemBazarRespostaDto MapearParaDto(ItemBazar item) =>
        new(
            item.Id,
            item.Nome,
            item.Descricao,
            item.ImagemUrl,
            item.Preco,
            item.QuantidadeDisponivel,
            item.Categoria,
            item.Condicao,
            item.Status,
            item.DataPublicacao,
            item.VendedorId,
            item.Vendedor?.ChavePix // Adicionado: pega o PIX caso o vendedor esteja carregado
        );

    private async Task RemoverImagemAntigaAsync(string? urlAntiga)
    {
        if (string.IsNullOrWhiteSpace(urlAntiga)) return;

        var publicId = _imageStorage.ExtrairPublicId(urlAntiga);
        if (publicId is not null)
        {
            await _imageStorage.DeleteAsync(publicId);
        }
    }
}
