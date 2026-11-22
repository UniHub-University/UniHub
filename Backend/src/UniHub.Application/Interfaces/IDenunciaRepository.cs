using System.Threading.Tasks;
using UniHub.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace UniHub.Application.Interfaces
{
    public interface IDenunciaRepository
    {
        Task<Denuncia> CriarAsync(Denuncia denuncia);
        // Como alteração futura, podemos adicionar Task<IEnumerable<Denuncia>> ListarPendentesAsync(); para o admin

        /// Retorna a página solicitada e o total de denúncias encontradas.
        /// O total permite montar os metadados da paginação.
        Task<(IReadOnlyList<Denuncia> Denuncias, int Total)> ListarParaAdminAsync(
            int page,
            int pageSize);

        /// Verifica se já existe denúncia em andamento (Aberta ou EmAnalise) do mesmo
        /// denunciante, para o mesmo denunciado e o mesmo produto (ProdutoId comparado
        /// por igualdade, incluindo o caso nulo). Usado para bloquear duplicata.
        Task<bool> ExisteDenunciaEmAndamentoAsync(System.Guid denuncianteId, System.Guid denunciadoId, System.Guid? produtoId);

        /// Obtém a denúncia como entidade rastreada, para que uma alteração de status
        /// seja persistida com SaveChangesAsync. Retorna null se não existir.
        Task<Denuncia?> ObterRastreadaAsync(System.Guid id);

        /// Persiste as alterações pendentes (ex.: status alterado via AtualizarStatus
        /// numa entidade obtida por ObterRastreadaAsync).
        Task SalvarAlteracoesAsync();
    }
}
