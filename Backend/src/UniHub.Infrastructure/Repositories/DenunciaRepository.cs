using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniHub.Application.Interfaces;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Infrastructure.Repositories
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

        /// <summary>
        /// Busca uma página de denúncias para o painel administrativo.
        /// Esta consulta é somente de leitura, por isso não rastreia alterações.
        /// </summary>
        public async Task<(IReadOnlyList<Denuncia> Denuncias, int Total)> ListarParaAdminAsync(
            int page,
            int pageSize)
        {
            // Conta todos os registros antes de aplicar a paginação.
            var total = await _context.Denuncias
                .AsNoTracking()
                .CountAsync();

            // Carrega os usuários e o produto para que o controller possa
            // montar os nomes resolvidos na resposta.
            var denuncias = await _context.Denuncias
                .AsNoTracking()
                .Include(d => d.Denunciante)
                .Include(d => d.Denunciado)
                .Include(d => d.Produto)
                // Mais recentes primeiro; o Id desempata datas iguais.
                .OrderByDescending(d => d.CriadaEm)
                .ThenBy(d => d.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (denuncias, total);
        }
    }
}