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

    public DenunciaStatus Status { get; set; } = DenunciaStatus.Aberta;

    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;
}
