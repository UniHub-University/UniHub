namespace UniHub.Application.DTOs.Vendas;

// Perfil PUBLICO de um vendedor (qualquer usuario autenticado pode ver).
// LGPD: expoe so o que ajuda o comprador a decidir. NUNCA inclui telefone,
// e-mail, UsuarioId, GoogleId nem a chave PIX (o PIX so aparece no anuncio).
public record VendedorPublicoDto(
    Guid Id,
    string Nome,
    string? FotoUrl,
    string? CardapioUrl,
    string? DescricaoNegocio,
    decimal NotaMedia,
    int QtdAvaliacoes,
    List<LocalVendaPublicoDto> Locais,
    List<HorarioVendaPublicoDto> Horarios
);

public record LocalVendaPublicoDto(string Campus, string PontoDeEncontro);

// DiaSemana: 0 = Domingo ... 6 = Sabado. Horas no formato "HH:mm".
public record HorarioVendaPublicoDto(DiaSemana DiaSemana, string HoraInicio, string HoraFim);
