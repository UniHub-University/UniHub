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

    }

}