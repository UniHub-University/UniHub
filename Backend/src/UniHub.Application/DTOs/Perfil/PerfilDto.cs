using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Perfil;

public record PerfilRespostaDto(
    Guid Id,
    string NomeCompleto,
    string EmailInstitucional,
    string Telefone,
    string FotoPerfilUrl,
    string Role,
    bool EhVendedor,
    bool EhVoluntario,
    DateTime CreatedAt
)
{
    /// <summary>
    /// Monta o DTO a partir da entidade, sem expor GoogleId nem qualquer
    /// outro dado sensível. IMPORTANTE: o Usuario passado aqui precisa
    /// ter sido carregado com .Include(u => u.Vendedor) e
    /// .Include(u => u.Voluntario) -- sem isso, EhVendedor/EhVoluntario
    /// retornam false mesmo que o usuário tenha esses perfis no banco,
    /// silenciosamente, sem lançar exceção.
    /// </summary>
    public static PerfilRespostaDto DeEntidade(Usuario usuario) => new(
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
}
