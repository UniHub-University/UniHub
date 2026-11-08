using UniHub.Application.DTOs.Vendas;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Domain.Entities;

namespace UniHub.Application.Services.Vendas;

// Visao publica de vendedores. Aqui mora a regra do que PODE ser exposto
// (LGPD): o mapeamento para o DTO publico e o unico caminho de saida, entao
// dados como chave PIX, telefone e e-mail nunca saem por aqui.
public class VendedorPublicoService
{
    private readonly IVendedorPublicoRepository _repository;

    public VendedorPublicoService(IVendedorPublicoRepository repository)
    {
        _repository = repository;
    }

    // Perfil publico do vendedor, ou null se nao existir (o Controller devolve 404).
    public async Task<VendedorPublicoDto?> ObterPerfilAsync(Guid vendedorId)
    {
        var vendedor = await _repository.ObterPublicoAsync(vendedorId);
        return vendedor is null ? null : Mapear(vendedor);
    }

    private static VendedorPublicoDto Mapear(Vendedor vendedor) => new(
        vendedor.Id,
        string.IsNullOrWhiteSpace(vendedor.Usuario?.NomeCompleto) ? "Vendedor" : vendedor.Usuario!.NomeCompleto,
        // Foto da loja; se nao houver, a foto de perfil do usuario; se nao houver, nenhuma.
        PrimeiroPreenchido(vendedor.FotoUrl, vendedor.Usuario?.FotoPerfilUrl),
        PrimeiroPreenchido(vendedor.CardapioUrl),
        PrimeiroPreenchido(vendedor.DescricaoNegocio),
        vendedor.NotaMedia,
        vendedor.QtdAvaliacoes,
        vendedor.Locais
            .OrderBy(l => l.Campus)
            .ThenBy(l => l.PontoDeEncontro)
            .Select(l => new LocalVendaPublicoDto(l.Campus, l.PontoDeEncontro))
            .ToList(),
        vendedor.Horarios
            .OrderBy(h => h.DiaSemana)
            .ThenBy(h => h.HoraInicio)
            .Select(h => new HorarioVendaPublicoDto(
                h.DiaSemana,
                h.HoraInicio.ToString(@"hh\:mm"),
                h.HoraFim.ToString(@"hh\:mm")))
            .ToList());

    private static string? PrimeiroPreenchido(params string?[] valores) =>
        valores.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
