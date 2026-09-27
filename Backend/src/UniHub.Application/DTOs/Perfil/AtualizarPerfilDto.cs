namespace UniHub.Application.DTOs.Perfil;

public class AtualizarPerfilDto
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    
    // O frontend envia a URL nova gerada pelo POST /api/upload
    // ou envia a mesma URL atual do Google caso não queira mudar a foto
    public string? FotoUrl { get; set; } 
}