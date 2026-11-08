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
}
