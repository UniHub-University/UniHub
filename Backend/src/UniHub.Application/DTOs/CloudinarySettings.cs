namespace UniHub.Application.DTOs;

/// Mapeado da secao "Cloudinary" do appsettings.json via IOptions.
/// Os valores reais vem de user-secrets (local) ou variaveis de
/// ambiente (producao) -- nunca do arquivo versionado.
public class CloudinarySettings
{
    public string CloudName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
 
    /// Tamanho maximo aceito, em megabytes.
    public int TamanhoMaximoMb { get; set; } = 3;
 
    /// Extensoes permitidas. Validadas junto com o content-type real
    /// do arquivo -- checar so a extensao do nome nao e suficiente,
    /// porque e trivial renomear um arquivo qualquer para .jpg.
    public string[] ExtensoesPermitidas { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
}
 
public record UploadImagemRespostaDto(string Url, string PublicId);
 