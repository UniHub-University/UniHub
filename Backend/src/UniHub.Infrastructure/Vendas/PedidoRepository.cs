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

    // P7: o dono vem do token (UsuarioId), nunca da requisicao. Produto.VendedorId
    // e o id do Vendedor, entao a ligacao com o usuario passa por Vendedor.UsuarioId.
    public async Task<bool> PedidoTemItemDoVendedorAsync(Guid pedidoId, Guid usuarioId) =>
        await _context.Set<Pedido>()
            .AsNoTracking()
            .AnyAsync(p => p.Id == pedidoId &&
                p.Itens.Any(i => i.Produto!.Vendedor!.UsuarioId == usuarioId));

    // P7: UPDATE condicional (WHERE Status = Reservado), no mesmo espirito do
    // decremento atomico de estoque. Se o job de expiracao ou outra requisicao
    // mudar o pedido antes, nenhuma linha e afetada e a baixa e recusada.
    public async Task<bool> TryConcluirAsync(Guid pedidoId)
    {
        var agora = DateTime.UtcNow;

        var linhasAfetadas = await _context.Set<Pedido>()
            .Where(p => p.Id == pedidoId && p.Status == PedidoStatus.Reservado)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Status, PedidoStatus.Concluido)
                .SetProperty(p => p.UpdatedAt, (DateTime?)agora));

        return linhasAfetadas == 1;
    }

    // P6: pedidos que tem ao menos um item de produto do vendedor. O vendedor vem
    // do token (usuarioId), nunca da requisicao. Consulta somente leitura (AsNoTracking).
    public async Task<(IReadOnlyList<Pedido> Pedidos, int Total)> GetByVendedorAsync(
        Guid usuarioId, int page, int pageSize)
    {
        var pedidosDoVendedor = _context.Set<Pedido>()
            .AsNoTracking()
            .Where(p => p.Itens.Any(i => i.Produto!.Vendedor!.UsuarioId == usuarioId));

        var total = await pedidosDoVendedor.CountAsync();

        var pedidos = await pedidosDoVendedor
            .Include(p => p.Comprador)
            .Include(p => p.Itens)
                .ThenInclude(i => i.Produto)
                    .ThenInclude(pr => pr!.Vendedor)
            .OrderByDescending(p => p.CreatedAt)
            .ThenByDescending(p => p.Id) // desempate: paginas estaveis
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (pedidos, total);
    }

    // P4: reservas vencidas, com os itens (ProdutoId + Quantidade) para o Service
    // devolver o estoque. Somente leitura (AsNoTracking): a gravacao e feita pelo
    // UPDATE condicional em TryExpirarAsync.
    public async Task<IReadOnlyList<Pedido>> GetReservasVencidasAsync(DateTime agora) =>
        await _context.Set<Pedido>()
            .AsNoTracking()
            .Include(p => p.Itens)
            .Where(p => p.Tipo == TipoPedido.Reserva
                && p.Status == PedidoStatus.Reservado
                && p.ExpiraEm != null
                && p.ExpiraEm <= agora)
            .ToListAsync();

    // P4/P5: UPDATE condicional (WHERE Status = Reservado), no mesmo espirito do
    // TryConcluirAsync. Se o pedido mudou de estado antes (conclusao manual, ou
    // outra expiracao concorrente), nenhuma linha e afetada e o chamador NAO
    // devolve o estoque -- evitando devolucao dupla.
    public async Task<bool> TryExpirarAsync(Guid pedidoId)
    {
        var agora = DateTime.UtcNow;

        var linhasAfetadas = await _context.Set<Pedido>()
            .Where(p => p.Id == pedidoId && p.Status == PedidoStatus.Reservado)
            .ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Status, PedidoStatus.Expirado)
                .SetProperty(p => p.UpdatedAt, (DateTime?)agora));

        return linhasAfetadas == 1;
    }
}