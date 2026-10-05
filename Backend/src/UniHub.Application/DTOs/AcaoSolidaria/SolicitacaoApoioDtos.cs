using System;
using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.AcaoSolidaria;

public record CriarSolicitacaoApoioDto(
    string Categoria,
    string Descricao
);

// DTO público: expõe apenas o necessário sem vazar dados do Solicitante
public record SolicitacaoApoioRespostaDto(
    Guid Id,
    string CodigoPublico,
    string Categoria,
    string Descricao,
    StatusSolicitacao Status,
    DateTime CreatedAt
);