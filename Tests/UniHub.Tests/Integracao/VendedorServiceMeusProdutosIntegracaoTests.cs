using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using UniHub.Application.Interfaces;
using UniHub.Application.Services;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;
using Xunit;

namespace UniHub.Tests.Integracao;

/// <summary>
/// Testes de integração do VendedorService.ListarMeusProdutosAsync contra a
/// branch development do Neon (mesmo padrão de VendedorServiceIntegracaoTests:
/// AppDbContext real, IImageStorageService mockado, transação + ROLLBACK por teste).
///
/// Foco: (1) o isolamento por VendedorId — um vendedor nunca vê produto de outro;
/// (2) a inclusão de TODOS os status (Disponível, Esgotado, Inativo), que a busca
/// pública esconde; (3) o mapeamento do Tipo ("lanche" para Alimento, "bazar" para
/// ItemBazar); (4) o caso "usuário sem perfil de vendedor".
/// </summary>
[Trait("Categoria", "Integracao")]
public class VendedorServiceMeusProdutosIntegracaoTests : IntegracaoTestBase
{
    // O ListarMeusProdutosAsync não usa o IImageStorageService, mas o construtor
    // do VendedorService exige: mock vazio só para instanciar.
    private readonly IImageStorageService _imagem = Substitute.For<IImageStorageService>();

    private static Usuario NovoUsuario() => new()
    {
        NomeCompleto = "Vendedor Teste",
        EmailInstitucional = $"teste-{Guid.NewGuid():N}@unifesp.br",
        IsActive = true
    };

    // Cria um usuário já com perfil de vendedor e devolve o service + o VendedorId.
    private async Task<(VendedorService service, Guid usuarioId, Guid vendedorId)> ComVendedor(AppDbContext context)
    {
        var usuario = NovoUsuario();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var service = new VendedorService(context, _imagem);
        await service.TornarSeVendedorAsync(usuario.Id);

        var vendedorId = await context.Vendedores
            .Where(v => v.UsuarioId == usuario.Id)
            .Select(v => v.Id)
            .FirstAsync();

        return (service, usuario.Id, vendedorId);
    }

    private static ItemBazar NovoItemBazar(Guid vendedorId, string nome, ProdutoStatus status = ProdutoStatus.Disponivel) => new()
    {
        Nome = nome,
        Descricao = "Descrição de teste",
        Preco = 10m,
        QuantidadeDisponivel = 5,
        Categoria = "Livro",
        Condicao = CondicaoItem.Usado,
        VendedorId = vendedorId,
        Status = status,
        DataPublicacao = DateTime.UtcNow
    };

    private static Alimento NovoAlimento(Guid vendedorId, string nome) => new()
    {
        Nome = nome,
        Descricao = "Salgado de teste",
        Preco = 6m,
        QuantidadeDisponivel = 10,
        Categoria = "Salgado",
        HorarioDisponibilidade = new TimeSpan(18, 0, 0),
        VendedorId = vendedorId,
        Status = ProdutoStatus.Disponivel,
        DataPublicacao = DateTime.UtcNow
    };

    // ---------- Sem perfil de vendedor ----------
    [Fact]
    public async Task ListarMeusProdutos_QuandoSemPerfilDeVendedor_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var service = new VendedorService(context, _imagem);

        var resultado = await service.ListarMeusProdutosAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Equal("Usuário não possui um perfil de vendedor válido.", resultado.Mensagem);
        Assert.Empty(resultado.Produtos);

        await tx.RollbackAsync();
    }

    // ---------- Vendedor sem produtos ----------
    [Fact]
    public async Task ListarMeusProdutos_QuandoVendedorSemProdutos_RetornaListaVaziaComSucesso()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuarioId, _) = await ComVendedor(context);

        var resultado = await service.ListarMeusProdutosAsync(usuarioId);

        Assert.True(resultado.Sucesso);
        Assert.Empty(resultado.Produtos);

        await tx.RollbackAsync();
    }

    // ---------- Isolamento por vendedor (o teste mais importante) ----------
    [Fact]
    public async Task ListarMeusProdutos_RetornaSomenteProdutosDoProprioVendedor()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var (service, usuarioId, vendedorId) = await ComVendedor(context);
        var (_, _, outroVendedorId) = await ComVendedor(context);

        context.ItensBazar.Add(NovoItemBazar(vendedorId, "Meu Livro"));
        context.ItensBazar.Add(NovoItemBazar(outroVendedorId, "Livro do Outro"));
        await context.SaveChangesAsync();

        var resultado = await service.ListarMeusProdutosAsync(usuarioId);

        Assert.True(resultado.Sucesso);
        Assert.Single(resultado.Produtos);
        Assert.Equal("Meu Livro", resultado.Produtos[0].Nome);
        // garante que nada do outro vendedor vazou
        Assert.DoesNotContain(resultado.Produtos, p => p.Nome == "Livro do Outro");

        await tx.RollbackAsync();
    }

    // ---------- Inclui todos os status ----------
    [Fact]
    public async Task ListarMeusProdutos_IncluiProdutosEsgotadosEInativos()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuarioId, vendedorId) = await ComVendedor(context);

        context.ItensBazar.Add(NovoItemBazar(vendedorId, "Disponível", ProdutoStatus.Disponivel));
        context.ItensBazar.Add(NovoItemBazar(vendedorId, "Esgotado", ProdutoStatus.Esgotado));
        context.ItensBazar.Add(NovoItemBazar(vendedorId, "Inativo", ProdutoStatus.Inativo));
        await context.SaveChangesAsync();

        var resultado = await service.ListarMeusProdutosAsync(usuarioId);

        Assert.True(resultado.Sucesso);
        Assert.Equal(3, resultado.Produtos.Count);
        Assert.Contains(resultado.Produtos, p => p.Status == ProdutoStatus.Esgotado);
        Assert.Contains(resultado.Produtos, p => p.Status == ProdutoStatus.Inativo);

        await tx.RollbackAsync();
    }

    // ---------- Mapeamento do Tipo (bazar vs lanche) ----------
    [Fact]
    public async Task ListarMeusProdutos_MapeiaTipoBazarELanche()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuarioId, vendedorId) = await ComVendedor(context);

        context.ItensBazar.Add(NovoItemBazar(vendedorId, "Item de Bazar"));
        context.Set<Alimento>().Add(NovoAlimento(vendedorId, "Coxinha"));
        await context.SaveChangesAsync();

        var resultado = await service.ListarMeusProdutosAsync(usuarioId);

        Assert.True(resultado.Sucesso);
        Assert.Equal(2, resultado.Produtos.Count);

        var bazar = resultado.Produtos.Single(p => p.Nome == "Item de Bazar");
        var lanche = resultado.Produtos.Single(p => p.Nome == "Coxinha");
        Assert.Equal("bazar", bazar.Tipo);
        Assert.Equal("lanche", lanche.Tipo);

        await tx.RollbackAsync();
    }

    // ---------- Ordenação (mais recentes primeiro) ----------
    [Fact]
    public async Task ListarMeusProdutos_OrdenaDoMaisRecenteParaOMaisAntigo()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuarioId, vendedorId) = await ComVendedor(context);

        var antigo = NovoItemBazar(vendedorId, "Antigo");
        antigo.DataPublicacao = DateTime.UtcNow.AddDays(-2);
        var recente = NovoItemBazar(vendedorId, "Recente");
        recente.DataPublicacao = DateTime.UtcNow;

        context.ItensBazar.Add(antigo);
        context.ItensBazar.Add(recente);
        await context.SaveChangesAsync();

        var resultado = await service.ListarMeusProdutosAsync(usuarioId);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Recente", resultado.Produtos[0].Nome);
        Assert.Equal("Antigo", resultado.Produtos[1].Nome);

        await tx.RollbackAsync();
    }
}
