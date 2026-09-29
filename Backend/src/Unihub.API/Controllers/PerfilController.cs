using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs.Perfil; 
using UniHub.Application.Interfaces;
using UniHub.Infrastructure.Data; 

namespace UniHub.API.Controllers; 

[ApiController] 
[Route("api/perfil")] 
[Authorize] 
public class PerfilController : ControllerBase 
{ 
    private readonly AppDbContext _context;
    private readonly IImageStorageService _imageStorage;

    public PerfilController(AppDbContext context, IImageStorageService imageStorage) 
    { 
        _context = context; 
        _imageStorage = imageStorage;
    } 

    [HttpGet] 
    public async Task<ActionResult<PerfilRespostaDto>> ObterPerfil() 
    { 
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());

        var usuario = await _context.Usuarios
            .Include(u => u.Vendedor)
            .Include(u => u.Voluntario)
            .FirstOrDefaultAsync(u => u.Id == usuarioId); 

        if (usuario is null) 
            return NotFound(new { mensagem = "Usuario nao encontrado." }); 

        var resposta = new PerfilRespostaDto(
            usuario.Id, 
            usuario.NomeCompleto, 
            usuario.EmailInstitucional, 
            usuario.Telefone, 
            usuario.FotoPerfilUrl, 
            usuario.Role.ToString(), 
            usuario.Vendedor != null, 
            usuario.Voluntario != null,
             usuario.CreatedAt 
        ); 

        return Ok(resposta); 
    } 

    [HttpPut]
    public async Task<IActionResult> AtualizarPerfil([FromBody] AtualizarPerfilDto dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var usuario = await _context.Usuarios.FindAsync(usuarioId);

        if (usuario is null) 
            return NotFound(new { mensagem = "Usuário não encontrado." });

        // Valida se a URL é do Cloudinary (ignorando se for a foto original do Google)
        if (!string.IsNullOrWhiteSpace(dto.FotoUrl) && !dto.FotoUrl.Contains("googleusercontent.com"))
        {
            if (!_imageStorage.UrlPertenceAoProvedor(dto.FotoUrl))
                return BadRequest(new { mensagem = "A URL da foto não é válida." });
        }

        // Remove a foto antiga do Cloudinary se o usuário estiver enviando uma nova
        if (dto.FotoUrl != usuario.FotoPerfilUrl && _imageStorage.UrlPertenceAoProvedor(usuario.FotoPerfilUrl))
        {
            var publicIdAntigo = _imageStorage.ExtrairPublicId(usuario.FotoPerfilUrl);
            if (publicIdAntigo != null)
            {
                await _imageStorage.DeleteAsync(publicIdAntigo);
            }
        }

        usuario.AtualizarDados(dto.NomeCompleto, dto.Telefone, dto.FotoUrl ?? usuario.FotoPerfilUrl);
        
        await _context.SaveChangesAsync();

        return Ok(new { mensagem = "Perfil atualizado com sucesso!" });
    }
}