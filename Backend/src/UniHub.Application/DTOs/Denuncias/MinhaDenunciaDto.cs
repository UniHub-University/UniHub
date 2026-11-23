using System;
using UniHub.Domain.Entities;

namespace UniHub.Application.DTOs.Denuncias;

/// <summary>
/// Dados de uma denúncia para a tela "Denúncias feitas por mim" (do denunciante).
/// Só-leitura. Devolve nomes prontos para exibição — nunca IDs de usuário
/// (LGPD): o denunciante vê quem denunciou pelo nome, sem trafegar UsuarioId.
/// ProdutoNome fica nulo quando a denúncia é apenas sobre o perfil do vendedor.
/// </summary>

public record MinhaDenunciaDto(
    Guid Id,
    string DenunciadoNome,
    string? ProdutoNome,
    DenunciaMotivo Motivo,
    string Descricao,
    DenunciaStatus Status,
    DateTime CriadaEm
);