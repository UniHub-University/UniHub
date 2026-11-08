using NSubstitute;
using UniHub.Application.DTOs;
using UniHub.Application.DTOs.Vendas;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Application.Services.Vendas;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Application;

// Testes do perfil publico do vendedor (VendedorPublicoService.ObterPerfilAsync).
[Trait("Categoria", "Unit")]
public class VendedorPublicoServiceTests
{
    private readonly IVendedorPublicoRepository _repository = Substitute.For<IVendedorPublicoRepository>();

    private VendedorPublicoService CriarService() => new(_repository);

    private static Vendedor NovoVendedor() => new()
    {
        Id = Guid.NewGuid(),
        UsuarioId = Guid.NewGuid(),
        Usuario = new Usuario
        {
            NomeCompleto = "Ana Souza",
            EmailInstitucional = "ana@unifesp.br",
            Telefone = "12999990000",
            FotoPerfilUrl = "https://exemplo.com/perfil.jpg"
        },
        FotoUrl = "https://exemplo.com/loja.jpg",
        CardapioUrl = "https://exemplo.com/cardapio.jpg",
        DescricaoNegocio = "Doces e salgados",
        ChavePix = "chave-pix-secreta"
    };

    // ---------- 404 ----------
    [Fact]
    public async Task ObterPerfil_QuandoVendedorNaoExiste_RetornaNull()
    {
        var id = Guid.NewGuid();
        _repository.ObterPublicoAsync(id).Returns((Vendedor?)null);

        var resultado = await CriarService().ObterPerfilAsync(id);

        Assert.Null(resultado);
    }

    // ---------- mapeamento ----------
    [Fact]
    public async Task ObterPerfil_MapeiaOsDadosPublicosDoVendedor()
    {
        var vendedor = NovoVendedor();
        vendedor.ReceberAvaliacao(5);
        vendedor.ReceberAvaliacao(4);
        _repository.ObterPublicoAsync(vendedor.Id).Returns(vendedor);

        var resultado = await CriarService().ObterPerfilAsync(vendedor.Id);

        Assert.NotNull(resultado);
        Assert.Equal(vendedor.Id, resultado.Id);
        Assert.Equal("Ana Souza", resultado.Nome);
        Assert.Equal("https://exemplo.com/loja.jpg", resultado.FotoUrl);
        Assert.Equal("https://exemplo.com/cardapio.jpg", resultado.CardapioUrl);
        Assert.Equal("Doces e salgados", resultado.DescricaoNegocio);
        Assert.Equal(4.5m, resultado.NotaMedia);
        Assert.Equal(2, resultado.QtdAvaliacoes);
    }

    // ---------- foto: cai para a foto de perfil do usuario ----------
    [Fact]
    public async Task ObterPerfil_SemFotoDaLoja_UsaFotoDePerfilDoUsuario()
    {
        var vendedor = NovoVendedor();
        vendedor.FotoUrl = null;
        _repository.ObterPublicoAsync(vendedor.Id).Returns(vendedor);

        var resultado = await CriarService().ObterPerfilAsync(vendedor.Id);

        Assert.Equal("https://exemplo.com/perfil.jpg", resultado!.FotoUrl);
    }

    [Fact]
    public async Task ObterPerfil_SemNenhumaFoto_DevolveFotoNula()
    {
        var vendedor = NovoVendedor();
        vendedor.FotoUrl = null;
        vendedor.Usuario!.FotoPerfilUrl = string.Empty;
        _repository.ObterPublicoAsync(vendedor.Id).Returns(vendedor);

        var resultado = await CriarService().ObterPerfilAsync(vendedor.Id);

        Assert.Null(resultado!.FotoUrl);
    }

    // ---------- locais e horarios ----------
    [Fact]
    public async Task ObterPerfil_FormataHorariosEOrdenaPorDiaEHora()
    {
        var vendedor = NovoVendedor();
        vendedor.Horarios.Add(new HorarioVenda
        {
            DiaSemana = DiaSemana.Quarta,
            HoraInicio = new TimeSpan(14, 0, 0),
            HoraFim = new TimeSpan(16, 30, 0)
        });
        vendedor.Horarios.Add(new HorarioVenda
        {
            DiaSemana = DiaSemana.Segunda,
            HoraInicio = new TimeSpan(9, 5, 0),
            HoraFim = new TimeSpan(11, 0, 0)
        });
        vendedor.Locais.Add(new LocalVenda { Campus = "SJC", PontoDeEncontro = "Bloco B" });
        vendedor.Locais.Add(new LocalVenda { Campus = "SJC", PontoDeEncontro = "Bloco A" });
        _repository.ObterPublicoAsync(vendedor.Id).Returns(vendedor);

        var resultado = await CriarService().ObterPerfilAsync(vendedor.Id);

        Assert.Equal(2, resultado!.Horarios.Count);
        Assert.Equal(DiaSemana.Segunda, resultado.Horarios[0].DiaSemana);
        Assert.Equal("09:05", resultado.Horarios[0].HoraInicio);
        Assert.Equal("11:00", resultado.Horarios[0].HoraFim);
        Assert.Equal(DiaSemana.Quarta, resultado.Horarios[1].DiaSemana);
        Assert.Equal("14:00", resultado.Horarios[1].HoraInicio);
        Assert.Equal("16:30", resultado.Horarios[1].HoraFim);

        Assert.Equal(2, resultado.Locais.Count);
        Assert.Equal("Bloco A", resultado.Locais[0].PontoDeEncontro);
        Assert.Equal("Bloco B", resultado.Locais[1].PontoDeEncontro);
    }

    // ---------- LGPD: o DTO publico nao tem campos sensiveis ----------
    [Fact]
    public void VendedorPublicoDto_NaoTemCamposSensiveis()
    {
        var campos = typeof(VendedorPublicoDto).GetProperties().Select(p => p.Name).ToList();

        Assert.DoesNotContain("ChavePix", campos);
        Assert.DoesNotContain("Telefone", campos);
        Assert.DoesNotContain("EmailInstitucional", campos);
        Assert.DoesNotContain("UsuarioId", campos);
        Assert.DoesNotContain("GoogleId", campos);
    }
}
