using System;
using System.Collections.Generic;

namespace UniHub.Domain.Entities;

public class Voluntario
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string CodigoPublico { get; private set; } = GerarCodigoPublico();

    public List<string> AreasDeAtuacao { get; set; } = new(); // ex: "Aulas de Reforço", "Doação de Roupas"
    public bool Disponivel { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Correspondencia> Correspondencias { get; set; } = new List<Correspondencia>();

    public void PausarAtuacao() => Disponivel = false;
    public void RetomarAtuacao() => Disponivel = true;

    private static string GerarCodigoPublico() =>
        "VOL-" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
}
