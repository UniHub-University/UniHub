using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs;

/// Versão mínima de Usuario para exibir como referência dentro de outras
/// respostas (ex: quem se candidatou a uma SolicitacaoApoio, quem
/// publicou um ItemBazar). Não inclui telefone/e-mail -- só o necessário
/// para identificar a pessoa na tela, minimizando exposição de dado.
public class UsuarioResumoDTO
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string FotoPerfilUrl { get; set; } = string.Empty;

    public static UsuarioResumoDTO DeEntidade(Usuario usuario) => new()
    {
        Id = usuario.Id,
        NomeCompleto = usuario.NomeCompleto,
        FotoPerfilUrl = usuario.FotoPerfilUrl
    };
}
