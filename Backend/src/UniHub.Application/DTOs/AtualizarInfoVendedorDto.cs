using System;

namespace UniHub.Application.DTOs;

public record AtualizarInfoVendedorDto(
    string? FotoUrl,
    string? CardapioUrl,
    string? DescricaoNegocio,
    string? ChavePix // Novo campo adicionado para a Sprint 3
);

public record InfoVendedorResultDto(bool Sucesso, string Mensagem);

public record TornarVendedorResultDto(bool Sucesso, string Mensagem, Guid? VendedorId);