namespace UniHub.Application.DTOs.Vendas;

// Resultado da baixa manual de um pedido pelo vendedor (P7).
// O Controller traduz o Status em HTTP: Sucesso 200, NaoEncontrado 404,
// Proibido 403 e Conflito 409.
public enum ConcluirPedidoStatus
{
    Sucesso,
    NaoEncontrado,
    Proibido,
    Conflito
}

public record ConcluirPedidoResultDto(ConcluirPedidoStatus Status, string Mensagem);