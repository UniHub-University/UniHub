using CloudinaryDotNet.Actions;
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
    public async Task<bool> DesativarAsync (Guid produtoId)
    {
        var produto = await _context.Set<Produto>().FirstOrDefaultAsync(p => p.Id == produtoId);

        if (produto == null)
        {
            return false;
        }

        produto.Status = ProdutoStatus.Inativo;
        await _context.SaveChangesAsync();

        return true;
    }
}