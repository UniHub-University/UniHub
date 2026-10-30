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

    public async Task<VoluntarioResultDto> DesistirAsync(Guid usuarioId)
    {
        var voluntario = await _context.Voluntarios
            .FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);

        if (voluntario is null)
            return Falha(SemPerfil);

        // Guarda: não pode desistir com apoio em andamento (compromisso ativo).
        var temAtiva = await _context.Correspondencias.AnyAsync(c =>
            c.VoluntarioId == voluntario.Id &&
            c.Status == StatusCorrespondencia.AceitaPeloVoluntario);

        if (temAtiva)
            return Falha("Você tem apoios em andamento. Conclua ou cancele antes de desistir.");

        _context.Voluntarios.Remove(voluntario);
        await _context.SaveChangesAsync();

        return new VoluntarioResultDto(true, "Você deixou de ser voluntário.", null);
    }

}
