using System;

namespace UniHub.Domain.Entities;

public enum StatusCorrespondencia
{
    Sugerida = 0,
    AceitaPeloVoluntario = 1,
    Concluida = 2,
    Cancelada = 3
}

/// Match entre um Voluntario e uma SolicitacaoApoio.
/// A identidade do solicitante só é revelada se ele autorizar explicitamente,
/// e só depois do aceite do voluntário.
public class Correspondencia
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SolicitacaoId { get; set; }
    public SolicitacaoApoio? Solicitacao { get; set; }

    public Guid VoluntarioId { get; set; }
    public Voluntario? Voluntario { get; set; }

    public StatusCorrespondencia Status { get; private set; } = StatusCorrespondencia.Sugerida;

    public bool IdentidadeRevelada { get; private set; }
    public DateTime? IdentidadeReveladaEm { get; private set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AceitaEm { get; private set; }

    public void Aceitar()
    {
        if (Solicitacao is null)
            throw new InvalidOperationException("Solicitacao precisa estar carregada para aceitar a correspondencia.");
        if (Status != StatusCorrespondencia.Sugerida)
            throw new InvalidOperationException("Só é possível aceitar uma correspondência sugerida.");

        Solicitacao.MarcarEmAndamento(); // falha se outro voluntário já aceitou
        Status = StatusCorrespondencia.AceitaPeloVoluntario;
        AceitaEm = DateTime.UtcNow;
    }

    public void AutorizarRevelacaoIdentidade()
    {
        if (Status != StatusCorrespondencia.AceitaPeloVoluntario)
            throw new InvalidOperationException("Identidade só pode ser revelada após o aceite do voluntário.");

        IdentidadeRevelada = true;
        IdentidadeReveladaEm = DateTime.UtcNow;
    }

    public void Concluir()
    {
        if (Status != StatusCorrespondencia.AceitaPeloVoluntario)
            throw new InvalidOperationException("Correspondência precisa estar aceita para ser concluída.");

        Status = StatusCorrespondencia.Concluida;
        Solicitacao?.Concluir();
    }

    public void Cancelar()
    {
        if (Status == StatusCorrespondencia.Concluida)
            throw new InvalidOperationException("Não é possível cancelar uma correspondência já concluída.");
        Status = StatusCorrespondencia.Cancelada;
    }
}
