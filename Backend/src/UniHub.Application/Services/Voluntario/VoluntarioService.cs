using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs.Voluntario;
using UniHub.Infrastructure.Data;

namespace UniHub.Application.Services;

// Orquestra o perfil de Voluntario. Espelha o VendedorService: usa o AppDbContext
// diretamente (sem repositorio) e devolve sempre um DTO de resultado, sem deixar
// a excecao de dominio (InvalidOperationException de Usuario.TornarSeVoluntario)
// estourar -- a checagem de duplicado acontece antes.
public class VoluntarioService
{
    private readonly AppDbContext _context;

    public VoluntarioService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<TornarVoluntarioResultDto> TornarSeVoluntarioAsync(Guid usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Voluntario)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario is null)
            return new TornarVoluntarioResultDto(false, "Usuário não encontrado.", null);

        if (usuario.Voluntario is not null)
            return new TornarVoluntarioResultDto(false, "Usuário já possui um perfil de voluntário.", usuario.Voluntario.Id);

        var voluntario = usuario.TornarSeVoluntario();

        _context.Voluntarios.Add(voluntario);
        await _context.SaveChangesAsync();

        return new TornarVoluntarioResultDto(true, "Perfil de voluntário criado com sucesso!", voluntario.Id);
    }
}
