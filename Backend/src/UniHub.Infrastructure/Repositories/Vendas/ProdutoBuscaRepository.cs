using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs.Common;
using UniHub.Application.DTOs.Produtos;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Infrastructure.Repositories.Vendas;

public class ProdutoBuscaRepository : IProdutoBuscaRepository
{
    // Valores da coluna discriminadora da TPH em "Produtos". Usamos a coluna
    // direto porque Alimento ainda nao esta mapeado no EF (filtrar "lanche"
    // retorna vazio ate o mapeamento + migration entrarem, via o Guardiao).
    private const string DiscriminadorBazar = "ItemBazar";
    private const string DiscriminadorAlimento = "Alimento";

    private readonly AppDbContext _context;

    public ProdutoBuscaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResultDto<ProdutoResumoDto>> BuscarAsync(FiltrosBuscaProdutos f)
    {
        // Base: so produtos a venda, de vendedores com conta ativa.
        var query = _context.Produtos
            .AsNoTracking()
            .Where(p => p.Status == ProdutoStatus.Disponivel
                     && p.QuantidadeDisponivel > 0
                     && p.Vendedor!.Usuario!.IsActive);

        if (!string.IsNullOrWhiteSpace(f.Termo))
        {
            var padrao = $"%{f.Termo}%";
            query = query.Where(p => EF.Functions.ILike(p.Nome, padrao)
                                  || (p.Descricao != null && EF.Functions.ILike(p.Descricao, padrao)));
        }

        if (f.Tipo.HasValue)
        {
            var discriminador = f.Tipo == TipoProduto.Lanche ? DiscriminadorAlimento : DiscriminadorBazar;
            query = query.Where(p => EF.Property<string>(p, "Discriminator") == discriminador);
        }

        if (f.PrecoMin.HasValue)
            query = query.Where(p => p.Preco >= f.PrecoMin.Value);

        if (f.PrecoMax.HasValue)
            query = query.Where(p => p.Preco <= f.PrecoMax.Value);

        if (!string.IsNullOrWhiteSpace(f.Campus))
        {
            var padraoCampus = $"%{f.Campus}%";
            query = query.Where(p => p.Vendedor!.Locais.Any(l => EF.Functions.ILike(l.Campus, padraoCampus)));
        }

        // Horario: o vendedor precisa ter uma janela que cubra o dia e/ou a
        // hora pedidos. Se so o dia vier, ignora a hora e vice-versa.
        if (f.DiaSemana.HasValue || f.Hora.HasValue)
        {
            var dia = f.DiaSemana;
            var hora = f.Hora;
            query = query.Where(p => p.Vendedor!.Horarios.Any(h =>
                (dia == null || h.DiaSemana == dia) &&
                (hora == null || (h.HoraInicio <= hora && h.HoraFim >= hora))));
        }

        var totalItems = await query.CountAsync();

        var itens = await query
            .OrderByDescending(p => p.DataPublicacao)
            .ThenBy(p => p.Id) // ordenacao estavel para a paginacao
            .Skip((f.Page - 1) * f.PageSize)
            .Take(f.PageSize)
            .Select(p => new ProdutoResumoDto(
                p.Id,
                p.Nome,
                p.Descricao,
                p.Preco,
                p.QuantidadeDisponivel,
                EF.Property<string>(p, "Discriminator") == DiscriminadorAlimento ? "lanche" : "bazar",
                p.DataPublicacao,
                p.VendedorId,
                p.Vendedor!.Usuario!.NomeCompleto,
                p.Vendedor.Locais.Select(l => l.Campus).Distinct().ToList()))
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalItems / (double)f.PageSize);

        return new PaginatedResultDto<ProdutoResumoDto>(itens, f.Page, f.PageSize, totalItems, totalPages);
    }
}
