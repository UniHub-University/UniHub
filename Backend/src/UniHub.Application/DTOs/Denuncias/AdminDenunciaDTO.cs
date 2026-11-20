using System;
using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Denuncias;

/// <summary>
/// Dados seguros e necessários para uma linha da lista administrativa.
/// Devolve nomes prontos para exibição, em vez dos objetos completos de usuário
/// ou produto. ProdutoNome fica nulo quando a denúncia é apenas sobre um perfil.
/// </summary>

public record AdminDenunciaDto(
    Guid Id,
    string DenuncianteNome,
    string DenunciadoNome,
    string? ProdutoNome,
    DenunciaMotivo Motivo,
    string Descricao,
    DenunciaStatus Status,
    DateTime CriadaEm
);
