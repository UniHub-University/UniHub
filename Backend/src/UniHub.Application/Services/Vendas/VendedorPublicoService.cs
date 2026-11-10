using UniHub.Application.DTOs.Common;
using UniHub.Application.DTOs.Vendas;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Domain.Entities;

namespace UniHub.Application.Services.Vendas;

// Visao publica de vendedores. Aqui mora a regra do que PODE ser exposto
// (LGPD): o mapeamento para os DTOs publicos e o unico caminho de saida, entao
// dados como chave PIX, telefone e e-mail nunca saem por aqui.
public class VendedorPublicoService
{
    private const int PageSizePadrao = 12;
    private const int PageSizeMaximo = 50;

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

    // Lista paginada de vendedores. Nunca confia em page/pageSize vindos da
    // requisicao: normaliza antes de consultar (padrao 12, maximo 50).
    public async Task<PaginatedResultDto<VendedorResumoDto>> ListarAsync(int page, int pageSize)
    {
        pageSize = pageSize < 1 ? PageSizePadrao : Math.Min(pageSize, PageSizeMaximo);

        // O teto de page evita estouro de int em (page - 1) * pageSize no repositorio.
        page = Math.Clamp(page, 1, int.MaxValue / pageSize);

        var (vendedores, total) = await _repository.ListarPublicosAsync(page, pageSize);

        var totalPaginas = (int)Math.Ceiling(total / (double)pageSize);

        return new PaginatedResultDto<VendedorResumoDto>(
            vendedores.Select(MapearResumo).ToList(),
            page,
            pageSize,
            total,
            totalPaginas);
    }

    private static VendedorResumoDto MapearResumo(Vendedor vendedor) => new(
        vendedor.Id,
        NomeDe(vendedor),
        FotoDe(vendedor),
        PrimeiroPreenchido(vendedor.DescricaoNegocio),
        vendedor.NotaMedia,
        vendedor.QtdAvaliacoes);

    private static VendedorPublicoDto Mapear(Vendedor vendedor) => new(
        vendedor.Id,
        NomeDe(vendedor),
        FotoDe(vendedor),
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

    private static string NomeDe(Vendedor vendedor) =>
        string.IsNullOrWhiteSpace(vendedor.Usuario?.NomeCompleto) ? "Vendedor" : vendedor.Usuario!.NomeCompleto;

    // Foto da loja; se nao houver, a foto de perfil do usuario; se nao houver, nenhuma.
    private static string? FotoDe(Vendedor vendedor) =>
        PrimeiroPreenchido(vendedor.FotoUrl, vendedor.Usuario?.FotoPerfilUrl);

    private static string? PrimeiroPreenchido(params string?[] valores) =>
        valores.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v));
}
