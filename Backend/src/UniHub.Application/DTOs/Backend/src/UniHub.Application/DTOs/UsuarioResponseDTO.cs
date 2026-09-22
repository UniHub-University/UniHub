using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs;

/// DTO de resposta completo para Usuario. Nunca inclui GoogleId -- esse é
/// o dado sensível que precisa ficar de fora de qualquer resposta da API,
/// equivalente funcional a nunca devolver senha/hash em sistemas com
/// login tradicional.
public class UsuarioResponseDTO
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string EmailInstitucional { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string FotoPerfilUrl { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool TemPerfilVendedor { get; set; }
    public bool TemPerfilVoluntario { get; set; }

    public static UsuarioResponseDTO DeEntidade(Usuario usuario) => new()
    {
        Id = usuario.Id,
        NomeCompleto = usuario.NomeCompleto,
        EmailInstitucional = usuario.EmailInstitucional,
        Telefone = usuario.Telefone,
        FotoPerfilUrl = usuario.FotoPerfilUrl,
        Role = usuario.Role.ToString(),
        CreatedAt = usuario.CreatedAt,
        TemPerfilVendedor = usuario.Vendedor != null,
        TemPerfilVoluntario = usuario.Voluntario != null
    };
}
