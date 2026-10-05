using System;
using System.Threading.Tasks;

namespace UniHub.Application.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<bool> SuspenderAsync(Guid id);
    }
}