namespace UniHub.Application.DTOs.Vendas;

// Resultado do cancelamento de uma reserva pelo comprador.
// O Controller traduz o Status em HTTP: Sucesso 200, NaoEncontrado 404,
// Proibido 403 e Conflito 409.
public enum CancelarReservaStatus
{
    Sucesso,
    NaoEncontrado,
    Proibido,
    Conflito
}

public record CancelarReservaResultDto(CancelarReservaStatus Status, string Mensagem);
