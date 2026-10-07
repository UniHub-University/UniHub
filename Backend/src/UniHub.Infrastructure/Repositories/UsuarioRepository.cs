using System;
using System.Threading.Tasks;
using UniHub.Application.Interfaces;
using UniHub.Infrastructure.Data;

namespace UniHub.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SuspenderAsync (Guid id)
        {
            // busca o usuário pelo ID
            var usuario = await _context.Usuarios.FindAsync(id);

            // se não encontrar, retorna falso
            if (usuario == null)
            {
                return false;
            }

            // executa a regra de negocio do dominii
            usuario.SuspenderConta();

            //salva a alteracao no banco de dados 
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ReativarAsync(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return false;
            }

            usuario.ReativarConta();

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}