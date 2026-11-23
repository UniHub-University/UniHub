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

        /// <summary>
        /// Há denúncia em andamento (Aberta/EmAnalise) do mesmo denunciante para o
        /// mesmo denunciado e o mesmo produto? ProdutoId é comparado por igualdade,
        /// o que cobre o caso nulo (denúncia só de perfil) como alvo distinto de uma
        /// denúncia sobre um produto específico.
        /// </summary>
        public async Task<bool> ExisteDenunciaEmAndamentoAsync(System.Guid denuncianteId, System.Guid denunciadoId, System.Guid? produtoId)
        {
            return await _context.Denuncias
                .AsNoTracking()
                .AnyAsync(d =>
                    d.DenuncianteId == denuncianteId &&
                    d.DenunciadoId == denunciadoId &&
                    d.ProdutoId == produtoId &&
                    (d.Status == DenunciaStatus.Aberta || d.Status == DenunciaStatus.EmAnalise));
        }

        /// <summary>
        /// Busca a denúncia rastreada (sem AsNoTracking) para permitir que a mudança
        /// de status feita por AtualizarStatus seja persistida com SaveChangesAsync.
        /// </summary>
        public async Task<Denuncia?> ObterRastreadaAsync(System.Guid id)
        {
            return await _context.Denuncias.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task SalvarAlteracoesAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Lista as denúncias feitas por um denunciante (tela "minhas denúncias").
        /// Somente leitura (AsNoTracking). Inclui Denunciado e Produto para o
        /// controller montar os nomes. Mais recentes primeiro; Id desempata.
        /// </summary>
        public async Task<IReadOnlyList<Denuncia>> ListarPorDenuncianteAsync(System.Guid denuncianteId)
        {
            return await _context.Denuncias
                .AsNoTracking()
                .Where(d => d.DenuncianteId == denuncianteId)
                .Include(d => d.Denunciado)
                .Include(d => d.Produto)
                .OrderByDescending(d => d.CriadaEm)
                .ThenBy(d => d.Id)
                .ToListAsync();
        }
    }
}
