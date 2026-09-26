namespace UniHub.Application.DTOs;

public record AtualizarInfoVendedorDto(
    string? FotoUrl,
    string? CardapioUrl,
    string? DescricaoNegocio
);

public record InfoVendedorResultDto(bool Sucesso, string Mensagem);

public record TornarVendedorResultDto(bool Sucesso, string Mensagem, Guid? VendedorId);