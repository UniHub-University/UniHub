using Microsoft.EntityFrameworkCore;
using UniHub.Application.Interfaces.Alimentacao;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Infrastructure.Repositories.Alimentacao;

public class AlimentoRepository : IAlimentoRepository
{
    private readonly AppDbContext _context;

    public AlimentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Alimento>> ObterDisponiveisAsync()
    {
        return await _context.Alimentos
            .AsNoTracking()
            .Where(a =>
                a.Status == ProdutoStatus.Disponivel &&
                a.QuantidadeDisponivel > 0)
            .OrderByDescending(a => a.DataPublicacao)
            .ToListAsync();
    }

    public async Task<Alimento?> ObterPorIdAsync(Guid id)
    {
        return await _context.Alimentos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Alimento?> ObterPorIdComVendedorAsync(Guid id)
    {
        return await _context.Alimentos
            .AsNoTracking()
            .Include(a => a.Vendedor)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Alimento?> ObterDoVendedorAsync(
        Guid id,
        Guid vendedorId)
    {
        return await _context.Alimentos
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.VendedorId == vendedorId);
    }

    public async Task AdicionarAsync(Alimento alimento)
    {
        await _context.Alimentos.AddAsync(alimento);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Alimento alimento)
    {
        _context.Alimentos.Update(alimento);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> InativarAsync(Guid id)
    {
        var linhasAfetadas = await _context.Alimentos
            .Where(a => a.Id == id)
            .ExecuteUpdateAsync(update =>
                update.SetProperty(
                    alimento => alimento.Status,
                    ProdutoStatus.Inativo));

        return linhasAfetadas == 1;
    }

    public async Task<bool> ReativarAsync(Guid id)
    {
        var linhasAfetadas = await _context.Alimentos
            .Where(a =>
                a.Id == id &&
                a.QuantidadeDisponivel > 0)
            .ExecuteUpdateAsync(update =>
                update.SetProperty(
                    alimento => alimento.Status,
                    ProdutoStatus.Disponivel));

        return linhasAfetadas == 1;
    }

    public async Task<Guid?> ObterVendedorIdPorUsuarioAsync(Guid usuarioId)
    {
        return await _context.Vendedores
            .Where(v => v.UsuarioId == usuarioId)
            .Select(v => (Guid?)v.Id)
            .FirstOrDefaultAsync();
    }

}
