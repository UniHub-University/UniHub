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

    public async Task<bool> AtualizarLogisticaAsync(Guid usuarioId, AtualizarLogisticaDTO dto)
    {
        // Busca o vendedor pelo UsuarioId (e já traz os locais e horarios ligados nele)
        var vendedor = await _context.Vendedores
            .Include(v => v.Locais)
            .Include(v => v.Horarios)
            .FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);

        if (vendedor == null) return false;

        // Apaga a logística antiga para não duplicar dados
        _context.Set<LocalVenda>().RemoveRange(vendedor.Locais);
        _context.Set<HorarioVenda>().RemoveRange(vendedor.Horarios);

        // Adiciona os novos locais que vieram do DTO
        foreach (var localDto in dto.Locais)
        {
            vendedor.Locais.Add(new LocalVenda
            {
                Campus = localDto.Campus,
                PontoDeEncontro = localDto.PontoDeEncontro
            });
        }

        // Adiciona os novos horários que vieram do DTO
        foreach (var horarioDto in dto.Horarios)
        {
            vendedor.Horarios.Add(new HorarioVenda
            {
                DiaSemana = horarioDto.DiaSemana,
                HoraInicio = TimeSpan.Parse(horarioDto.HoraInicio),
                HoraFim = TimeSpan.Parse(horarioDto.HoraFim)
            });
        }

        await _context.SaveChangesAsync();
        return true;
    }
}