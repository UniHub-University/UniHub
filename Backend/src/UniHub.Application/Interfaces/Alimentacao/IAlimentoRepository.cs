using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniHub.Domain.Entities;

namespace UniHub.Application.Interfaces.Alimentacao;

public interface IAlimentoRepository
{
    Task<List<Alimento>> ObterDisponiveisAsync();
    Task<Alimento?> ObterPorIdAsync(Guid id);
    Task<Alimento?> ObterPorIdComVendedorAsync(Guid id);
    Task<Alimento?> ObterDoVendedorAsync(Guid id, Guid vendedorId);
    Task<Guid?> ObterVendedorIdPorUsuarioAsync(Guid usuarioId);
    Task AdicionarAsync(Alimento alimento);
    Task AtualizarAsync(Alimento alimento);
    Task<bool> InativarAsync(Guid id);
    Task<bool> ReativarAsync(Guid id);
}