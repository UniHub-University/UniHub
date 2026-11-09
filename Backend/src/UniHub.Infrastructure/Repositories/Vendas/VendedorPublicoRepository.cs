using Microsoft.EntityFrameworkCore;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Infrastructure.Repositories.Vendas;

public class VendedorPublicoRepository : IVendedorPublicoRepository
{
    private readonly AppDbContext _context;

    public VendedorPublicoRepository(AppDbContext context)
    {
        _context = context;
    }

    // Somente leitura (AsNoTracking). AsSplitQuery evita o produto cartesiano
    // entre Locais e Horarios (duas colecoes no mesmo Include).
    // Usuario inativo some da visao publica.
    public async Task<Vendedor?> ObterPublicoAsync(Guid vendedorId) =>
        await _context.Vendedores
            .AsNoTracking()
            .AsSplitQuery()
            .Include(v => v.Usuario)
            .Include(v => v.Locais)
            .Include(v => v.Horarios)
            .Where(v => v.Id == vendedorId && v.Usuario!.IsActive)
            .FirstOrDefaultAsync();

    // Lista publica: so vendedores ativos que tem algo a venda agora. O filtro vem
    // antes do CountAsync para o total e a paginacao tambem descartarem os demais.
    public async Task<(IReadOnlyList<Vendedor> Vendedores, int Total)> ListarPublicosAsync(
        int page, int pageSize)
    {
        var listaveis = _context.Vendedores
            .AsNoTracking()
            .Where(v => v.Usuario!.IsActive
                && _context.Produtos.Any(p =>
                    p.VendedorId == v.Id
                    && p.Status == ProdutoStatus.Disponivel
                    && p.QuantidadeDisponivel > 0));

        var total = await listaveis.CountAsync();

        var vendedores = await listaveis
            .Include(v => v.Usuario)
            .OrderByDescending(v => v.NotaMedia)
            .ThenByDescending(v => v.QtdAvaliacoes)
            .ThenBy(v => v.Id) // desempate: paginas estaveis
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (vendedores, total);
    }
}
