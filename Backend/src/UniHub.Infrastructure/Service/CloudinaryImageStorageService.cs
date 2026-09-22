using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UniHub.Application.DTOs;
using UniHub.Application.Interfaces;

namespace UniHub.Infrastructure.Services;

public class CloudinaryImageStorageService : IImageStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly CloudinarySettings _settings;
    private readonly ILogger<CloudinaryImageStorageService> _logger;

    private static readonly Dictionary<string, byte[][]> AssinaturasValidas = new()
    {
        [".jpg"] = [[0xFF, 0xD8, 0xFF]],
        [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
        [".png"] = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],
        [".webp"] = [[0x52, 0x49, 0x46, 0x46]]
    };

    public CloudinaryImageStorageService(
        IOptions<CloudinarySettings> settings,
        ILogger<CloudinaryImageStorageService> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.CloudName) ||
            string.IsNullOrWhiteSpace(_settings.ApiKey) ||
            string.IsNullOrWhiteSpace(_settings.ApiSecret))
        {
            throw new InvalidOperationException(
                "Credenciais do Cloudinary nao configuradas. Configure via user-secrets " +
                "(Cloudinary:CloudName, Cloudinary:ApiKey, Cloudinary:ApiSecret).");
        }

        var account = new Account(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<string> UploadAsync(IFormFile arquivo, string pasta, CancellationToken cancellationToken = default)
    {
        ValidarArquivo(arquivo);

        await using var stream = arquivo.OpenReadStream();

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(arquivo.FileName, stream),
            Folder = $"unihub/{pasta}",
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false,
            // Free Tier: 800x800 + quality/format auto reduz consumo de
            // storage e bandwidth (cota compartilhada de 25 creditos/mes).
            Transformation = new Transformation()
                .Width(800)
                .Height(800)
                .Crop("limit")
                .Quality("auto")
                .FetchFormat("auto")
        };

        var resultado = await _cloudinary.UploadAsync(uploadParams, cancellationToken);

        if (resultado.Error is not null)
        {
            _logger.LogError("Falha no upload para o Cloudinary: {Erro}", resultado.Error.Message);
            throw new InvalidOperationException("Nao foi possivel enviar a imagem. Tente novamente.");
        }

        return resultado.SecureUrl.ToString();
    }

    public async Task<bool> DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(publicId)) return false;

        var resultado = await _cloudinary.DestroyAsync(new DeletionParams(publicId));

        if (resultado.Error is not null)
        {
            _logger.LogWarning("Falha ao remover imagem {PublicId}: {Erro}", publicId, resultado.Error.Message);
            return false;
        }

        return resultado.Result == "ok";
    }

    public string? ExtrairPublicId(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;

        try
        {
            var uri = new Uri(url);
            var segmentos = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            var indiceUpload = Array.IndexOf(segmentos, "upload");
            if (indiceUpload < 0 || indiceUpload + 1 >= segmentos.Length) return null;

            var inicio = indiceUpload + 1;
            if (segmentos[inicio].StartsWith('v') && segmentos[inicio][1..].All(char.IsDigit))
                inicio++;

            if (inicio >= segmentos.Length) return null;

            var caminho = string.Join('/', segmentos[inicio..]);
            var ultimoPonto = caminho.LastIndexOf('.');

            return ultimoPonto > 0 ? caminho[..ultimoPonto] : caminho;
        }
        catch (UriFormatException)
        {
            return null;
        }
    }

    /// Confirma que a URL pertence ao cloud configurado deste projeto --
    /// usado para validar URLs recebidas de volta do cliente (ex: em
    /// AtualizarInfoVendedorDto), evitando aceitar URL externa arbitraria.
    public bool UrlPertenceAoProvedor(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return false;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;

        var hostEsperado = "res.cloudinary.com";
        var caminhoEsperado = $"/{_settings.CloudName}/";

        return uri.Host.Equals(hostEsperado, StringComparison.OrdinalIgnoreCase)
            && uri.AbsolutePath.StartsWith(caminhoEsperado, StringComparison.OrdinalIgnoreCase);
    }

    private void ValidarArquivo(IFormFile arquivo)
    {
        if (arquivo is null || arquivo.Length == 0)
            throw new ArgumentException("Nenhum arquivo foi enviado.");

        var tamanhoMaximoBytes = _settings.TamanhoMaximoMb * 1024L * 1024L;
        if (arquivo.Length > tamanhoMaximoBytes)
            throw new ArgumentException($"A imagem excede o tamanho maximo de {_settings.TamanhoMaximoMb}MB.");

        var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
        if (!_settings.ExtensoesPermitidas.Contains(extensao))
        {
            var permitidas = string.Join(", ", _settings.ExtensoesPermitidas);
            throw new ArgumentException($"Formato nao permitido. Use: {permitidas}.");
        }

        if (!ConteudoCorrespondeAExtensao(arquivo, extensao))
            throw new ArgumentException("O conteudo do arquivo nao corresponde a um formato de imagem valido.");
    }

    private static bool ConteudoCorrespondeAExtensao(IFormFile arquivo, string extensao)
    {
        if (!AssinaturasValidas.TryGetValue(extensao, out var assinaturas))
            return false;

        using var stream = arquivo.OpenReadStream();
        var maiorAssinatura = assinaturas.Max(a => a.Length);
        var cabecalho = new byte[maiorAssinatura];

        var lidos = stream.Read(cabecalho, 0, maiorAssinatura);
        if (lidos < maiorAssinatura) return false;

        return assinaturas.Any(assinatura =>
            cabecalho.Take(assinatura.Length).SequenceEqual(assinatura));
    }
}