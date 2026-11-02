using Microsoft.EntityFrameworkCore;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Infrastructure.Repositories.Vendas;

public class ProdutoRepository : IProdutoRepository
{
    private readonly AppDbContext _context;

    public ProdutoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Produto?> GetByIdAsync(Guid id) =>
        await _context.Set<Produto>().AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    // O produto pertence ao usuario quando o Vendedor dono (Produto.VendedorId ->
    // Vendedor.Id) tem UsuarioId igual ao usuario informado. Consulta so leitura.
    public async Task<bool> ProdutoPertenceAoUsuarioAsync(Guid produtoId, Guid usuarioId) =>
        await _context.Set<Produto>()
            .AsNoTracking()
            .Join(
                _context.Vendedores.AsNoTracking(),
                produto => produto.VendedorId,
                vendedor => vendedor.Id,
                (produto, vendedor) => new { produto.Id, vendedor.UsuarioId })
            .AnyAsync(x => x.Id == produtoId && x.UsuarioId == usuarioId);

    public async Task<bool> TryDecrementarEstoqueAsync(Guid produtoId, int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("A quantidade deve ser positiva.", nameof(quantidade));

        var linhasAfetadas = await _context.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE ""Produtos""
               SET ""QuantidadeDisponivel"" = ""QuantidadeDisponivel"" - {quantidade}
               WHERE ""Id"" = {produtoId} AND ""QuantidadeDisponivel"" >= {quantidade}");

        return linhasAfetadas == 1;
    }

    public async Task IncrementarEstoqueAsync(Guid produtoId, int quantidade)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE ""Produtos""
               SET ""QuantidadeDisponivel"" = ""QuantidadeDisponivel"" + {quantidade}
               WHERE ""Id"" = {produtoId}");
    }

    public async Task<bool> InativarAsync(Guid produtoId)
    {
        var linhas = await _context.Set<Produto>()
            .Where(p => p.Id == produtoId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, ProdutoStatus.Inativo));
        return linhas == 1;
    }
}