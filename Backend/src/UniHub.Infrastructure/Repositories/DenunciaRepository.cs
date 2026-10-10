using System.Threading.Tasks;
using UniHub.Application.Interfaces;
using UniHub.Domain.Entities;
using UniHub.Infraestructure.Data;

namespace uniHub.Infrastructure.Repositories
{
    public class DenunciaRepository : IDenunciaRepository
    {
        private readonly AppDbContext _context;

        public DenunciaRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Denuncia> CriarAsync(Denuncia denuncia)
        {
            _context.Denuncias.Add(denuncia);
            await _context.SaveChangesAsync();
            return denuncia;
        }
    }
}