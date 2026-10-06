// Backend/src/Unihub.Application/DTOs/Solidariedade/DoacaoDtos.cs
using System.ComponentModel.DataAnnotations;
using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Solidariedade;

public class CriarDoacaoDto
{
    [Required]
    public Guid SolicitacaoId { get; set; }

    [Required, StringLength(500, MinimumLength = 3)]
    public string Descricao { get; set; } = string.Empty;
}

/// REGRA DE PRIVACIDADE (LGPD): este DTO só tem o CodigoPublico da solicitação.
/// Nunca adicionar Nome, Email, SolicitanteId, Solicitante etc. aqui.
/// Há um teste (DoacaoTests) que quebra se alguém fizer isso.
public record DoacaoRespostaDto(
    Guid Id,
    string CodigoPublicoSolicitacao,
    string Descricao,
    StatusDoacao Status,
    DateTime CreatedAt,
    DateTime? EntregueEm);
