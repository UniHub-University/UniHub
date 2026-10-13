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