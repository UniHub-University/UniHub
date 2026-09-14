using System;

namespace UniHub.Domain.Entities;

public enum StatusCorrespondencia
{
    Sugerida = 0,             // match calculado, ninguém confirmou ainda
    AceitaPeloVoluntario = 1, // voluntário confirmou que vai atender
    Concluida = 2,
    Cancelada = 3
}

/// Representa o match entre um Voluntario e uma SolicitacaoApoio.
/// A revelação de identidade fica simplificada aqui como um flag
/// (IdentidadeRevelada) que só pode ser ligado depois do aceite, e
/// exige que o próprio solicitante autorize explicitamente via
/// AutorizarRevelacaoIdentidade() -- não é automático ao aceitar.
public class Correspondencia
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SolicitacaoId { get; set; }
    public SolicitacaoApoio? Solicitacao { get; set; }

    public Guid VoluntarioId { get; set; }
    public Voluntario? Voluntario { get; set; }

    public StatusCorrespondencia Status { get; private set; } = StatusCorrespondencia.Sugerida;

    public bool IdentidadeRevelada { get; private set; } = false;
    public DateTime? IdentidadeReveladaEm { get; private set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AceitaEm { get; private set; }

    public void Aceitar()
    {
        if (Status != StatusCorrespondencia.Sugerida)
            throw new InvalidOperationException("Só é possível aceitar uma correspondência sugerida.");

        Status = StatusCorrespondencia.AceitaPeloVoluntario;
        AceitaEm = DateTime.UtcNow;
        if (Solicitacao is null)
              throw new InvalidOperationException("Solicitacao precisa estar carregada (via Include) para aceitar a correspondencia.");
        Solicitacao.MarcarEmAndamento();
    }

    /// Chamado explicitamente pelo solicitante (nunca automaticamente).
    /// É o ponto central de privacy by design deste módulo: sem essa
    /// chamada, o voluntário nunca tem acesso aos dados reais do aluno.
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
    }

    public void Cancelar()
{
    if (Status == StatusCorrespondencia.Concluida)
        throw new InvalidOperationException("Nao e possivel cancelar uma correspondencia ja concluida.");
    Status = StatusCorrespondencia.Cancelada;
}
}
