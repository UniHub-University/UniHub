// UniHub.Domain/Entities/SolicitacaoApoio.cs
using System;
using System.Collections.Generic;

namespace UniHub.Domain.Entities;

public enum StatusSolicitacao
{
    Aberta = 0,
    EmAndamento = 1,
    Concluida = 2,
    Cancelada = 3
}

public class SolicitacaoApoio
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SolicitanteId { get; set; }
    public Usuario? Solicitante { get; set; }

    // pseudônimo público, gerado uma vez -- ver padrão Value Object.
    // "private set" porque nada, nem o Service, deveria conseguir
    // trocar esse código depois de criado.
    public string CodigoPublico { get; private set; } = GerarCodigoPublico();

    public string Categoria { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    public StatusSolicitacao Status { get; private set; } = StatusSolicitacao.Aberta;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Correspondencia> Correspondencias { get; set; } = new List<Correspondencia>();
    public ICollection<Doacao> Doacoes { get; set; } = new List<Doacao>();

    /// Chamado internamente quando uma Correspondencia é aceita.
    /// Fica package-visible via método, não via setter público, para
    /// não deixar qualquer código externo forçar esse estado sem passar
    /// pela regra de negócio (ex: sem correspondência aceita de verdade).
    public void MarcarEmAndamento()
    {
        if (Status != StatusSolicitacao.Aberta)
            throw new InvalidOperationException("Só uma solicitação aberta pode entrar em andamento.");
        Status = StatusSolicitacao.EmAndamento;
    }

    public void Concluir()
    {
        if (Status != StatusSolicitacao.EmAndamento)
            throw new InvalidOperationException("Só é possível concluir uma solicitação em andamento.");
        Status = StatusSolicitacao.Concluida;
    }

    public void Cancelar()
    {
        if (Status == StatusSolicitacao.Concluida)
            throw new InvalidOperationException("Não é possível cancelar uma solicitação já concluída.");
        Status = StatusSolicitacao.Cancelada;
    }

    private static string GerarCodigoPublico() =>
        "SOL-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
}
