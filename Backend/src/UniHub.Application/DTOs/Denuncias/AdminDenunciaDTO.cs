using System;
using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Denuncias;

/// <summary>
/// Dados de uma linha da lista administrativa de denúncias.
/// Devolve nomes prontos para exibição. DenunciadoId e ProdutoId são usados
/// pelo painel para as AÇÕES de moderação (suspender/reativar/inativar) — dado
/// operacional de um endpoint restrito a Admin; a UI exibe só os nomes, nunca os ids.
/// (DenuncianteId é omitido de propósito: o admin não age sobre quem denunciou.)
/// </summary>
public record AdminDenunciaDto(
    Guid Id,
    Guid DenunciadoId,
    string DenuncianteNome,
    string DenunciadoNome,
    Guid? ProdutoId,
    string? ProdutoNome,
    DenunciaMotivo Motivo,
    string Descricao,
    DenunciaStatus Status,
    DateTime CriadaEm
);
