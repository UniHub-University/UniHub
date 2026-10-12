using System;
using System.Threading.Tasks;
using UniHub.Domain.Entities;

namespace UniHub.Application.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<bool> SuspenderAsync(Guid id);
        Task<bool> ReativarAsync(Guid id);
        Task<Usuario?> ObterPorIdAsync(Guid id);
    }

    
}