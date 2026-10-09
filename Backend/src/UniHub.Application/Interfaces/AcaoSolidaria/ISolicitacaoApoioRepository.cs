namespace UniHub.Application.Interfaces.AcaoSolidaria;

public interface ISolicitacaoApoioRepository
{
    /// <summary>
    /// Tenta marcar a solicitação como EmAndamento de forma atômica.
    /// Só altera se ela ainda estiver Aberta, garantindo que apenas UM
    /// voluntário vence a corrida de aceite, mesmo com requisições simultâneas.
    /// Retorna true se este chamador venceu (1 linha afetada); false se outro
    /// já aceitou antes (0 linhas) ou se a solicitação não existe.
    /// </summary>
    Task<bool> TryMarcarEmAndamentoAsync(Guid solicitacaoId);
}
