using System.Threading.Tasks;
using UniHub.Domain.Entities;

namespace UniHub.Application.Interfaces
{
    public interface IDenunciaRepository
    {
        Task<Denuncia> CriarAsync(Denuncia denuncia);
        // Como alteração futura, podemos adicionar Task<IEnumerable<Denuncia>> ListarPendentesAsync(); para o admin
    }
}