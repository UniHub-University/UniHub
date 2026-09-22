using Microsoft.AspNetCore.Http;

namespace UniHub.Application.Interfaces;

public interface IImageStorageService
{
    Task<string> UploadAsync(IFormFile arquivo, string pasta, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken = default);

    string? ExtrairPublicId(string url);

    /* Confirma que a URL pertence ao provedor/cloud configurado deste
     projeto -- usado para validar URLs recebidas de volta do cliente
    (ex: em AtualizarInfoVendedorDto), evitando que o endpoint aceite
    qualquer URL externa arbitraria como se fosse uma imagem legitima
    enviada via upload. */

    bool UrlPertenceAoProvedor(string url);
}