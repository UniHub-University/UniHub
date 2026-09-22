using System;

namespace UniHub.Domain.Entities;

public enum StatusDoacao
{
    Pendente = 0, //Voluntario se comprometeu, ainda não entregou
    Concluida = 1,
    Cancelada = 2
}

/// Representa o registro de uma doacao concreta (item ou ajuda) feita
/// em resposta a uma SolicitacaoApoio. Diferente de Correspondencia
/// (que eh o "match" entre voluntario e solicitacao), Doacao registra
/// o que de fato foi doado -- pode existir mais de uma Doacao para a
/// mesma SolicitacaoApoio, se a ajuda vier em partes.

public class Doacao
{
    public Guid Id {get; set;} = Guid.NewGuid();

    public Guid SolicitacaoId {get; set;}
    public SolicitacaoApoio? Solicitacao {get; set;}

    // Quem doou. Nullable de proposito: se a identidade do voluntario
    // ja e conhecida do sistema (ele tem uma Correspondencia aceita),
    // referenciamos aqui; mas o nome do doador so deve ser exposto ao
    // solicitante seguindo a mesma regra de revelacao de identidade
    // que ja vale para o outro lado (ver Correspondencia).

    public Guid VoluntarioId {get; set;}
    public Voluntario? Voluntario {get; set;}


    public string Descricao {get; set;} = string.Empty;

    public StatusDoacao Status {get; private set;} = StatusDoacao.Pendente;

    public DateTime CreatedAt {get; set;} = DateTime.UtcNow;
    public DateTime? EntregueEm {get; private set; }

    public void ConfirmarEntrega()
    {
        if(Status != StatusDoacao.Pendente)
            throw new InvalidOperationException("Só uma doação prometida pode ser marcada como entregue.");
            Status = StatusDoacao.Concluida;
            EntregueEm = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        if(Status == StatusDoacao.Concluida)
            throw new InvalidOperationException("Não é possível cancelar uma doação já entregue.");
            Status = StatusDoacao.Cancelada;
    }

}