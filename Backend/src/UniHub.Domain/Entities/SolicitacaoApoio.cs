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

    // pseudônimo público, gerado uma vez
    public string CodigoPublico { get; private set; } = GerarCodigoPublico();

    public CategoriaApoio Categoria { get; set; }
    public string Descricao { get; set; } = string.Empty;

    public StatusSolicitacao Status { get; private set; } = StatusSolicitacao.Aberta;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // token de concorrência (vira a coluna xmin do PostgreSQL)

    public ICollection<Correspondencia> Correspondencias { get; set; } = new List<Correspondencia>();
    public ICollection<Doacao> Doacoes { get; set; } = new List<Doacao>();

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
