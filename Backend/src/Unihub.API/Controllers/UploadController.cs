using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.DTOs;
using UniHub.Application.Interfaces;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/upload")]
[Authorize] // apenas usuarios autenticados com e-mail institucional
public class UploadController : ControllerBase
{
    private readonly IImageStorageService _imageStorage;

    // Pastas aceitas. Lista fechada de proposito: sem isso, o cliente
    // poderia passar qualquer string e poluir a organizacao do
    // Cloudinary (ou tentar path traversal com "../").
    private static readonly HashSet<string> PastasPermitidas =
        new(StringComparer.OrdinalIgnoreCase) { "perfis", "produtos", "cardapios" };

    public UploadController(IImageStorageService imageStorage)
    {
        _imageStorage = imageStorage;
    }

    /// POST /api/upload/imagem?pasta=perfis
    /// Envia o arquivo como multipart/form-data e retorna a URL publica.
    /// A URL retornada e o que deve ser salvo nas entidades
    /// (Usuario.FotoPerfilUrl, Vendedor.FotoUrl, Vendedor.CardapioUrl, etc.).
    [HttpPost("imagem")]
    [RequestSizeLimit(10 * 1024 * 1024)] // corta requisicoes absurdas antes de processar
    public async Task<ActionResult<UploadImagemRespostaDto>> EnviarImagem(
        IFormFile arquivo,
        [FromQuery] string pasta,
        CancellationToken cancellationToken)
    {
        if (!PastasPermitidas.Contains(pasta))
        {
            var permitidas = string.Join(", ", PastasPermitidas);
            return BadRequest(new { mensagem = $"Pasta invalida. Use uma destas: {permitidas}." });
        }

        try
        {
            var url = await _imageStorage.UploadAsync(arquivo, pasta.ToLowerInvariant(), cancellationToken);
            var publicId = _imageStorage.ExtrairPublicId(url);

            return Ok(new UploadImagemRespostaDto(url, publicId ?? string.Empty));
        }
        catch (ArgumentException ex)
        {
            // Erros de validacao (tamanho, formato) sao culpa da requisicao.
            return BadRequest(new { mensagem = ex.Message });
        }
    }

    /// DELETE /api/upload/imagem?publicId=unihub/perfis/abc123
    [HttpDelete("imagem")]
    public async Task<IActionResult> RemoverImagem(
        [FromQuery] string publicId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(publicId))
            return BadRequest(new { mensagem = "publicId e obrigatorio." });

        // ATENCAO (pendencia conhecida): hoje qualquer usuario autenticado
        // pode remover qualquer imagem, bastando saber o publicId. Nao expor
        // esse endpoint amplamente -- a remocao de imagens ja acontece
        // automaticamente pelo VendedorService.AtualizarInfoAsync quando a
        // URL e trocada.
        var removida = await _imageStorage.DeleteAsync(publicId, cancellationToken);

        return removida
            ? NoContent()
            : NotFound(new { mensagem = "Imagem nao encontrada ou ja removida." });
    }
}