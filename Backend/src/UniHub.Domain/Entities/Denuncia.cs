using System;

namespace UniHub.Domain.Entities;

public enum DenunciaStatus
{
    Aberta = 0,
    EmAnalise = 1,
    Resolvida = 2,
    Rejeitada = 3
}

public enum DenunciaMotivo
{
    ConteudoOfensivo = 0,
    ComportamentoInadequado = 1,
    Assedio = 2,
    Fraude = 3,
    ProdutoInadequado = 4,
    Outro = 5
}

public class Denuncia
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Usuário que fez a denúncia
    public Guid DenuncianteId { get; set; }
    public Usuario? Denunciante { get; set; }

    // Usuário denunciado
    public Guid DenunciadoId { get; set; }
    public Usuario? Denunciado { get; set; }

    // Produto relacionado (qnd houver)
    public Guid? ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public DenunciaMotivo Motivo { get; set; }

    public string Descricao { get; set; } = string.Empty;

    // Setter privado: a transição de status só pode passar por AtualizarStatus,
    // que aplica as regras de negócio. O EF Core consegue materializar mesmo
    // com setter privado.
    public DenunciaStatus Status { get; private set; } = DenunciaStatus.Aberta;

    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    // Transição de status da denúncia (painel de moderação).
    // Regras: Aberta e EmAnalise podem ir para qualquer status válido;
    // Resolvida e Rejeitada são finais e não aceitam mais nenhuma alteração.
    public void AtualizarStatus(DenunciaStatus novoStatus)
    {
        if (!Enum.IsDefined(typeof(DenunciaStatus), novoStatus))
            throw new ArgumentException("Status de denúncia inválido.", nameof(novoStatus));

        if (Status == DenunciaStatus.Resolvida || Status == DenunciaStatus.Rejeitada)
            throw new InvalidOperationException("Uma denúncia já resolvida ou rejeitada não pode mudar de status.");

        Status = novoStatus;
    }
}
