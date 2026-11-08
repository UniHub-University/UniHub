using Microsoft.EntityFrameworkCore;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Infrastructure.Data;

namespace UniHub.Infrastructure.Repositories.Vendas;

public class VendedorConsultaRepository : IVendedorConsultaRepository
{
    private readonly AppDbContext _context;

    public VendedorConsultaRepository(AppDbContext context)
    {
        _context = context;
    }

    // Projeta so o Id (nullable) para nao materializar a entidade inteira.
    // FirstOrDefaultAsync sobre Guid? devolve null quando o usuario nao tem
    // perfil de vendedor -- o chamador usa isso para decidir se agrupa a conexao.
    public async Task<Guid?> ObterVendedorIdPorUsuarioAsync(Guid usuarioId) =>
        await _context.Vendedores
            .AsNoTracking()
            .Where(v => v.UsuarioId == usuarioId)
            .Select(v => (Guid?)v.Id)
            .FirstOrDefaultAsync();
}
