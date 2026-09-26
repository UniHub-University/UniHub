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
            .FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);

        if (vendedor == null)
            return new LogisticaResultDto(false, "Usuário não possui um perfil de vendedor válido.");

        // 1. Validar e montar as novas listas ANTES de apagar qualquer coisa --
        // assim, se algo for invalido, o vendedor nao fica sem logistica
        // cadastrada por causa de uma validacao que falhou no meio do processo.
        var novosHorarios = new List<HorarioVenda>();
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

            novosHorarios.Add(new HorarioVenda
            {
                VendedorId = vendedor.Id,
                DiaSemana = horarioDto.DiaSemana,
                HoraInicio = horaInicio,
                HoraFim = horaFim
            });
        }

        var novosLocais = dto.Locais.Select(localDto => new LocalVenda
        {
            VendedorId = vendedor.Id,
            Campus = localDto.Campus,
            PontoDeEncontro = localDto.PontoDeEncontro
        }).ToList();

        // 2. So depois de tudo validado, apaga o antigo -- DELETE em massa
        // direto no banco (uma unica instrucao SQL por tabela), sem depender
        // de entidades rastreadas em memoria. Isso elimina o
        // DbUpdateConcurrencyException que estava ocorrendo antes.
        await _context.Set<LocalVenda>()
            .Where(l => l.VendedorId == vendedor.Id)
            .ExecuteDeleteAsync();

        await _context.Set<HorarioVenda>()
            .Where(h => h.VendedorId == vendedor.Id)
            .ExecuteDeleteAsync();

        // 3. Insere o novo.
        _context.Set<LocalVenda>().AddRange(novosLocais);
        _context.Set<HorarioVenda>().AddRange(novosHorarios);

        await _context.SaveChangesAsync();
        return new LogisticaResultDto(true, "Logística atualizada com sucesso.");
    }

    public async Task<TornarVendedorResultDto> TornarSeVendedorAsync(Guid usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Vendedor)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);
    
        if (usuario is null)
            return new TornarVendedorResultDto(false, "Usuário não encontrado.", null);
    
        if (usuario.Vendedor is not null)
            return new TornarVendedorResultDto(false, "Usuário já possui um perfil de vendedor.", usuario.Vendedor.Id);
    
        var vendedor = usuario.TornarSeVendedor();
    
        _context.Vendedores.Add(vendedor);
        await _context.SaveChangesAsync();
    
        return new TornarVendedorResultDto(true, "Perfil de vendedor criado com sucesso!", vendedor.Id);
    }

    
    /// Atualiza os dados descritivos da loja (foto, cardapio, descricao).
    /// Espera que FotoUrl/CardapioUrl ja tenham sido geradas previamente
    /// via POST /api/upload/imagem -- este metodo so persiste as URLs,
    /// nao faz upload.
    public async Task<InfoVendedorResultDto> AtualizarInfoAsync(Guid usuarioId, AtualizarInfoVendedorDto dto)
    {
        var vendedor = await _context.Vendedores.FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);

        if (vendedor == null)
            return new InfoVendedorResultDto(false, "Usuário não possui um perfil de vendedor válido.");

        // Valida que as URLs recebidas realmente vieram do Cloudinary configurado.
        if (!string.IsNullOrWhiteSpace(dto.FotoUrl) && !_imageStorage.UrlPertenceAoProvedor(dto.FotoUrl))
            return new InfoVendedorResultDto(false, "A URL da foto não pertence ao provedor de imagens configurado.");

        if (!string.IsNullOrWhiteSpace(dto.CardapioUrl) && !_imageStorage.UrlPertenceAoProvedor(dto.CardapioUrl))
            return new InfoVendedorResultDto(false, "A URL do cardápio não pertence ao provedor de imagens configurado.");

        // Se a foto ou cardapio mudaram, remove a imagem antiga do provedor
        // para nao deixar imagens orfas consumindo a cota gratuita.
        if (dto.FotoUrl is not null && dto.FotoUrl != vendedor.FotoUrl)
        {
            await RemoverImagemAntigaAsync(vendedor.FotoUrl);
            vendedor.FotoUrl = string.IsNullOrWhiteSpace(dto.FotoUrl) ? null : dto.FotoUrl;
        }

        if (dto.CardapioUrl is not null && dto.CardapioUrl != vendedor.CardapioUrl)
        {
            await RemoverImagemAntigaAsync(vendedor.CardapioUrl);
            vendedor.CardapioUrl = string.IsNullOrWhiteSpace(dto.CardapioUrl) ? null : dto.CardapioUrl;
        }

        if (dto.DescricaoNegocio is not null)
        {
            vendedor.DescricaoNegocio = dto.DescricaoNegocio;
        }

        await _context.SaveChangesAsync();
        return new InfoVendedorResultDto(true, "Informações da loja atualizadas com sucesso!");
    }

    private async Task RemoverImagemAntigaAsync(string? urlAntiga)
    {
        if (string.IsNullOrWhiteSpace(urlAntiga)) return;

        var publicId = _imageStorage.ExtrairPublicId(urlAntiga);
        if (publicId is not null)
        {
            await _imageStorage.DeleteAsync(publicId);
        }
    }
}
