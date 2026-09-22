namespace UniHub.Application.DTOs;

public record AtualizarInfoVendedorDto(
    string? FotoUrl,
    string? CardapioUrl,
    string? DescricaoNegocio
);

public record InfoVendedorResultDto(bool Sucesso, string Mensagem);