using NSubstitute;
using UniHub.Application.DTOs.Vendas;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Application.Services.Vendas;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Application;

// Testes da lista publica de vendedores (VendedorPublicoService.ListarAsync).
[Trait("Categoria", "Unit")]
public class VendedorPublicoListaTests
{
    private readonly IVendedorPublicoRepository _repository = Substitute.For<IVendedorPublicoRepository>();

    private VendedorPublicoService CriarService() => new(_repository);

    private static Vendedor NovoVendedor(string nome = "Ana Souza") => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = Guid.NewGuid(),
        Usuario = new Usuario
        {
            NomeCompleto = nome,
            EmailInstitucional = "ana@unifesp.br",
            Telefone = "12999990000",
            FotoPerfilUrl = "https://exemplo.com/perfil.jpg"
        },
        FotoUrl = "https://exemplo.com/loja.jpg",
        DescricaoNegocio = "Doces e salgados",
        ChavePix = "chave-pix-secreta"
    };

    private void RepositorioDevolve(int page, int pageSize, List<Vendedor> vendedores, int total)
    {
        (IReadOnlyList<Vendedor> Vendedores, int Total) retorno = (vendedores, total);
        _repository.ListarPublicosAsync(page, pageSize).Returns(retorno);
    }

    // ---------- mapeamento e paginacao ----------
    [Fact]
    public async Task Listar_MapeiaOsVendedoresEMontaAPaginacao()
    {
        var vendedor = NovoVendedor();
        vendedor.ReceberAvaliacao(5);
        vendedor.ReceberAvaliacao(4);
        RepositorioDevolve(1, 12, new List<Vendedor> { vendedor }, 25);

        var resultado = await CriarService().ListarAsync(1, 12);

        var item = Assert.Single(resultado.Items);
        Assert.Equal(vendedor.Id, item.Id);
        Assert.Equal("Ana Souza", item.Nome);
        Assert.Equal("https://exemplo.com/loja.jpg", item.FotoUrl);
        Assert.Equal("Doces e salgados", item.DescricaoNegocio);
        Assert.Equal(4.5m, item.NotaMedia);
        Assert.Equal(2, item.QtdAvaliacoes);

        Assert.Equal(1, resultado.Page);
        Assert.Equal(12, resultado.PageSize);
        Assert.Equal(25, resultado.TotalItems);
        Assert.Equal(3, resultado.TotalPages); // 25 / 12 arredondado para cima
    }

    [Fact]
    public async Task Listar_SemVendedores_DevolveListaVaziaComZeroPaginas()
    {
        RepositorioDevolve(1, 12, new List<Vendedor>(), 0);

        var resultado = await CriarService().ListarAsync(1, 12);

        Assert.Empty(resultado.Items);
        Assert.Equal(0, resultado.TotalItems);
        Assert.Equal(0, resultado.TotalPages);
    }

    [Fact]
    public async Task Listar_SemFotoDaLoja_UsaFotoDePerfilDoUsuario()
    {
        var vendedor = NovoVendedor();
        vendedor.FotoUrl = null;
        RepositorioDevolve(1, 12, new List<Vendedor> { vendedor }, 1);

        var resultado = await CriarService().ListarAsync(1, 12);

        Assert.Equal("https://exemplo.com/perfil.jpg", resultado.Items.Single().FotoUrl);
    }

    // ---------- validacao de page/pageSize (nunca confiar na requisicao) ----------
    [Theory]
    [InlineData(0, 0, 1, 12)]            // pageSize invalido -> padrao 12; page invalida -> 1
    [InlineData(-5, -1, 1, 12)]
    [InlineData(1, 1000, 1, 50)]         // pageSize acima do maximo -> 50
    [InlineData(3, 20, 3, 20)]           // valores validos passam como estao
    public async Task Listar_NormalizaPageEPageSizeAntesDeConsultar(
        int page, int pageSize, int pageEsperada, int pageSizeEsperado)
    {
        RepositorioDevolve(pageEsperada, pageSizeEsperado, new List<Vendedor>(), 0);

        var resultado = await CriarService().ListarAsync(page, pageSize);

        await _repository.Received(1).ListarPublicosAsync(pageEsperada, pageSizeEsperado);
        Assert.Equal(pageEsperada, resultado.Page);
        Assert.Equal(pageSizeEsperado, resultado.PageSize);
    }

    [Fact]
    public async Task Listar_PageGigante_EhLimitadaParaNaoEstourarOInt()
    {
        var teto = int.MaxValue / 12;
        RepositorioDevolve(teto, 12, new List<Vendedor>(), 0);

        var resultado = await CriarService().ListarAsync(int.MaxValue, 12);

        await _repository.Received(1).ListarPublicosAsync(teto, 12);
        Assert.Equal(teto, resultado.Page);
    }

    // ---------- LGPD: o DTO da lista nao tem campos sensiveis ----------
    [Fact]
    public void VendedorResumoDto_NaoTemCamposSensiveis()
    {
        var campos = typeof(VendedorResumoDto).GetProperties().Select(p => p.Name).ToList();

        Assert.DoesNotContain("ChavePix", campos);
        Assert.DoesNotContain("Telefone", campos);
        Assert.DoesNotContain("EmailInstitucional", campos);
        Assert.DoesNotContain("UsuarioId", campos);
        Assert.DoesNotContain("GoogleId", campos);
    }
}
