using System.ComponentModel.DataAnnotations;
using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.AcaoSolidaria;

public class AceitarSolicitacaoDto
{
    [Required(ErrorMessage = "O ID da solicitação é obrigatório.")]
    public Guid SolicitacaoId { get; set; }
}

// nao expor nome/telefone do solicitante aqui, só depois que ele autorizar
public record CorrespondenciaRespostaDto(
    Guid Id,
    string CodigoPublicoSolicitacao,
    CategoriaApoio CategoriaSolicitacao,
    StatusCorrespondencia Status,
    DateTime CreatedAt,
    DateTime? AceitaEm,
    bool IdentidadeRevelada);

// Visão do SOLICITANTE sobre uma correspondência das suas solicitações.
// Mostra o voluntário que aceitou pelo código público (pseudônimo). Nome e
// contato do voluntário só são preenchidos quando a identidade já foi revelada
// (até lá vêm null), espelhando o privacy-by-design do módulo.
public record MinhaCorrespondenciaDto(
    Guid Id,
    Guid SolicitacaoId,
    string CodigoPublicoSolicitacao,
    CategoriaApoio CategoriaSolicitacao,
    StatusCorrespondencia Status,
    bool IdentidadeRevelada,
    DateTime CreatedAt,
    DateTime? AceitaEm,
    string CodigoPublicoVoluntario,
    string? NomeVoluntario,
    string? ContatoVoluntario);