using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using UniHub.Application.DTOs;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Application.Services;

public class VendedorService
{
    private readonly AppDbContext _context;

    public VendedorService(AppDbContext context)
    {
        _context = context;
    }

   public async Task<LogisticaResultDto> AtualizarLogisticaAsync(Guid usuarioId, AtualizarLogisticaDTO dto)
    {
        var vendedor = await _context.Vendedores
            .Include(v => v.Locais)
            .Include(v => v.Horarios)
            .FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);

        if (vendedor == null)
            return new LogisticaResultDto(false, "Usuário não possui um perfil de vendedor válido.");

        _context.Set<LocalVenda>().RemoveRange(vendedor.Locais);
        _context.Set<HorarioVenda>().RemoveRange(vendedor.Horarios);

        foreach (var localDto in dto.Locais)
        {
            vendedor.Locais.Add(new LocalVenda
            {
                Campus = localDto.Campus,
                PontoDeEncontro = localDto.PontoDeEncontro
            });
        }

        foreach (var horarioDto in dto.Horarios)
        {
            if (!TimeSpan.TryParse(horarioDto.HoraInicio, out var horaInicio) ||
                !TimeSpan.TryParse(horarioDto.HoraFim, out var horaFim))
            {
                return new LogisticaResultDto(false, $"Horário inválido para '{horarioDto.DiaSemana}'. Use o formato HH:mm.");
            }

            if (horaInicio >= horaFim)
            {
                return new LogisticaResultDto(false, $"Horário de início deve ser antes do fim ('{horarioDto.DiaSemana}').");
            }

            vendedor.Horarios.Add(new HorarioVenda
            {
                DiaSemana = horarioDto.DiaSemana,
                HoraInicio = horaInicio,
                HoraFim = horaFim
            });
        }

        await _context.SaveChangesAsync();
        return new LogisticaResultDto(true, "Locais e horários atualizados com sucesso!");
    }
}