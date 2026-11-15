// Backend/src/Unihub.Application/DTOs/Solidariedade/DoacaoDtos.cs
using System.ComponentModel.DataAnnotations;
using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Solidariedade;

public class CriarDoacaoDto
{
    // Caminho preferido do front: a doação parte de uma correspondência ACEITA do
    // voluntário logado. O backend resolve a SolicitacaoId a partir dela, para o
    // front nunca manusear o id da solicitacao (so circula o codigo publico — LGPD).
    public Guid? CorrespondenciaId { get; set; }

    // Compatibilidade: ainda aceito o SolicitacaoId direto (contrato antigo).
    // Informar CorrespondenciaId OU SolicitacaoId (o controller valida).
    public Guid? SolicitacaoId { get; set; }

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
