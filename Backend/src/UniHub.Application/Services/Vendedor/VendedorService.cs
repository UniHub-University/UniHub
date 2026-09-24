using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using UniHub.Application.DTOs;
using UniHub.Application.Interfaces;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Application.Services;

public class VendedorService
{
    private readonly AppDbContext _context;
    private readonly IImageStorageService _imageStorage;

    public VendedorService(AppDbContext context, IImageStorageService imageStorage)
    {
        _context = context;
        _imageStorage = imageStorage;
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
        return new LogisticaResultDto(true, "Logística atualizada com sucesso.");

    }

    /* Atualiza os dados descritivos da loja (foto, cardapio, descricao).
    Espera que FotoUrl/CardapioUrl ja tenham sido geradas previamente
    via POST /api/upload/imagem -- este metodo so persiste as URLs,
     nao faz upload. */

    public async Task<InfoVendedorResultDto> AtualizarInfoAsync(Guid usuarioId, AtualizarInfoVendedorDto dto)
    {
        var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);

        if(vendedor == null)
            return new InfoVendedorResultDto(false, "Usuário não possui um perfil de vendedor válido.");

        // Validar se as URLs pertencem ao provedor de imagens configurado
        if(!string.IsNullOrWhiteSpace(dto.FotoUrl) && !_imageStorage.UrlPertenceAoProvedor(dto.FotoUrl))
            return new InfoVendedorResultDto(false, "A URL da foto não pertence ao provedor de imagens configurado.");
        
        if(!string.IsNullOrWhiteSpace(dto.CardapioUrl) && !_imageStorage.UrlPertenceAoProvedor(dto.CardapioUrl))
            return new InfoVendedorResultDto(false, "A URL do cardápio não pertence ao provedor de imagens configurado.");
        
        //Se a foto ou cardapio foram alterados, deletar a imagem antiga do provedor
        if(dto.FotoUrl is not null && dto.FotoUrl != vendedor.FotoUrl)
        {
            await RemoverImagemAntiga(vendedor.FotoUrl);
            vendedor.FotoUrl = string.IsNullOrWhiteSpace(dto.FotoUrl) ? null : dto.FotoUrl;
        }

        if(dto.CardapioUrl is not null && dto.CardapioUrl != vendedor.CardapioUrl)
        {
            await RemoverImagemAntiga(vendedor.CardapioUrl);
            vendedor.CardapioUrl = string.IsNullOrWhiteSpace(dto.CardapioUrl) ? null : dto.CardapioUrl;
        }

        if(dto.Descricao is not null)
        {
            vendedor.Descricao = dto.Descricao;
        }

        await _context.SaveChangesAsync();
        return new InfoVendedorResultDto(true, "Informações da loja atualizadas com sucesso!");
    }

    private async Task RemoverImagemAntiga(string? urlAntiga)
    {
        if(string.IsNullOrWhiteSpace(urlAntiga)) return;

        var publicId = _imageStorage.ExtrairPublicId(urlAntiga);
        if(publicId is not null)
        {
            await _imageStorage.DeleteAsync(publicId);
        }
    }
}
