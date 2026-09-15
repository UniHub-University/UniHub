using Microsoft.EntityFrameworkCore;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace Unihub.Infrastracture.Repositories.Vendas;

public class PedidoRepository :IPedidoRepository
{
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context){
        _context = context;
    }

    public async Task<Pedido> AddAsync(Pedido pedido){
        _context.Set<Pedido>().Add(pedido);
        await _context.SaveChangesAsync();
        return pedido;
    }

    public async Task<Pedido?> GetByIdAsync(Guid id) =>
        await _context.Set<Pedido>()
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);
}