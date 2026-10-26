using Microsoft.EntityFrameworkCore;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Infrastructure.Repositories.Vendas;

public class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Pedido> AddAsync(Pedido pedido)
    {
        _context.Set<Pedido>().Add(pedido);
        await _context.SaveChangesAsync();
        return pedido;
    }

    public async Task<Pedido?> GetByIdAsync(Guid id) =>
        await _context.Set<Pedido>()
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);

    // Historico de pedidos: so os pedidos do comprador informado, do mais
    // recente para o mais antigo, consulta somente leitura, por isso AsNoTracking.
    public async Task<(IReadOnlyList<Pedido> Pedidos, int Total)> GetByCompradorAsync(
        Guid compradorId, int page, int pageSize)
    {
        var pedidosDoComprador = _context.Set<Pedido>()
            .AsNoTracking()
            .Where(p => p.CompradorId == compradorId);

        var total = await pedidosDoComprador.CountAsync();

        var pedidos = await pedidosDoComprador
            .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id) // desempate: paginas estaveis se dois pedidos tiverem o mesmo CreatedAt
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (pedidos, total);
    }
}