using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using UniHub.Application.DTOs;
using UniHub.Application.Interfaces;
using UniHub.Application.Services;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;
using Xunit;

namespace UniHub.Tests.Integracao;

/// <summary>
/// Testes de integração do VendedorService contra a branch development do Neon.
///
/// Caso híbrido: o service depende de DUAS coisas —
///   1. AppDbContext (banco real) -> por isso é teste de integração;
///   2. IImageStorageService (interface do Cloudinary) -> mockada com NSubstitute,
///      para NÃO bater no Cloudinary de verdade nos testes.
///
/// Transação + ROLLBACK por teste (ver IntegracaoTestBase): nada fica persistido.
///
/// A validação de formato da chave PIX NÃO é re-testada aqui em todos os formatos
/// (isso é coberto por ChavePixValidatorTests). Aqui só exercitamos a integração
/// do service com o validator: inválida -> falha; válida -> persiste.
/// </summary>
public class VendedorServiceIntegracaoTests : IntegracaoTestBase
{
    // Mock do armazenamento de imagem. Por padrão aceita qualquer URL como
    // pertencente ao provedor; cada teste reconfigura quando precisa.
    private readonly IImageStorageService _imagem = Substitute.For<IImageStorageService>();

    public VendedorServiceIntegracaoTests()
    {
        _imagem.UrlPertenceAoProvedor(Arg.Any<string>()).Returns(true);
        _imagem.ExtrairPublicId(Arg.Any<string>()).Returns("public-id-fake");
        _imagem.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
    }

    private static Usuario NovoUsuario() => new()
    {
        NomeCompleto = "Vendedor Teste",
        EmailInstitucional = $"teste-{Guid.NewGuid():N}@unifesp.br",
        IsActive = true
    };

    // ---------- TornarSeVendedor ----------
    [Fact]
    public async Task TornarSeVendedor_QuandoUsuarioNaoExiste_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var service = new VendedorService(context, _imagem);

        var resultado = await service.TornarSeVendedorAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Equal("Usuário não encontrado.", resultado.Mensagem);
        Assert.Null(resultado.VendedorId);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task TornarSeVendedor_QuandoUsuarioValido_CriaPerfil()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var usuario = NovoUsuario();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var service = new VendedorService(context, _imagem);
        var resultado = await service.TornarSeVendedorAsync(usuario.Id);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.VendedorId);
        Assert.True(await context.Vendedores.AnyAsync(v => v.UsuarioId == usuario.Id));

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task TornarSeVendedor_QuandoJaTemPerfil_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var usuario = NovoUsuario();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var service = new VendedorService(context, _imagem);
        await service.TornarSeVendedorAsync(usuario.Id); // cria

        var resultado = await service.TornarSeVendedorAsync(usuario.Id); // de novo

        Assert.False(resultado.Sucesso);
        Assert.Equal("Usuário já possui um perfil de vendedor.", resultado.Mensagem);
        // mesmo na falha, devolve o Id do perfil existente
        Assert.NotNull(resultado.VendedorId);

        await tx.RollbackAsync();
    }

    // ---------- AtualizarLogistica ----------
    private async Task<(VendedorService service, Usuario usuario)> ComVendedorSetup(AppDbContext context)
    {
        var usuario = NovoUsuario();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        var service = new VendedorService(context, _imagem);
        await service.TornarSeVendedorAsync(usuario.Id);
        return (service, usuario);
    }

    [Fact]
    public async Task AtualizarLogistica_QuandoSemPerfil_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var service = new VendedorService(context, _imagem);

        var resultado = await service.AtualizarLogisticaAsync(Guid.NewGuid(), new AtualizarLogisticaDTO());

        Assert.False(resultado.Sucesso);
        Assert.Equal("Usuário não possui um perfil de vendedor válido.", resultado.Mensagem);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task AtualizarLogistica_QuandoHorarioMalFormatado_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuario) = await ComVendedorSetup(context);

        var dto = new AtualizarLogisticaDTO
        {
            Horarios = new List<HorarioVendaDTO>
            {
                new() { DiaSemana = DiaSemana.Segunda, HoraInicio = "abc", HoraFim = "18:00" }
            }
        };

        var resultado = await service.AtualizarLogisticaAsync(usuario.Id, dto);

        Assert.False(resultado.Sucesso);
        Assert.Contains("Horário inválido", resultado.Mensagem);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task AtualizarLogistica_QuandoInicioDepoisDoFim_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuario) = await ComVendedorSetup(context);

        var dto = new AtualizarLogisticaDTO
        {
            Horarios = new List<HorarioVendaDTO>
            {
                new() { DiaSemana = DiaSemana.Segunda, HoraInicio = "18:00", HoraFim = "12:00" }
            }
        };

        var resultado = await service.AtualizarLogisticaAsync(usuario.Id, dto);

        Assert.False(resultado.Sucesso);
        Assert.Contains("antes do fim", resultado.Mensagem);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task AtualizarLogistica_QuandoValido_PersisteLocaisEHorarios()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuario) = await ComVendedorSetup(context);
        var vendedor = await context.Vendedores.FirstAsync(v => v.UsuarioId == usuario.Id);

        var dto = new AtualizarLogisticaDTO
        {
            Locais = new List<LocalVendaDTO>
            {
                new() { Campus = "São José dos Campos", PontoDeEncontro = "Bloco A" }
            },
            Horarios = new List<HorarioVendaDTO>
            {
                new() { DiaSemana = DiaSemana.Segunda, HoraInicio = "12:00", HoraFim = "18:00" }
            }
        };

        var resultado = await service.AtualizarLogisticaAsync(usuario.Id, dto);

        Assert.True(resultado.Sucesso);
        Assert.Equal(1, await context.Set<LocalVenda>().CountAsync(l => l.VendedorId == vendedor.Id));
        Assert.Equal(1, await context.Set<HorarioVenda>().CountAsync(h => h.VendedorId == vendedor.Id));

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task AtualizarLogistica_QuandoChamadoDuasVezes_SubstituiLogisticaAnterior()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuario) = await ComVendedorSetup(context);
        var vendedor = await context.Vendedores.FirstAsync(v => v.UsuarioId == usuario.Id);

        var dto1 = new AtualizarLogisticaDTO
        {
            Locais = new List<LocalVendaDTO>
            {
                new() { Campus = "Campus 1", PontoDeEncontro = "Ponto 1" },
                new() { Campus = "Campus 2", PontoDeEncontro = "Ponto 2" }
            }
        };
        await service.AtualizarLogisticaAsync(usuario.Id, dto1); // 2 locais

        var dto2 = new AtualizarLogisticaDTO
        {
            Locais = new List<LocalVendaDTO>
            {
                new() { Campus = "Campus 3", PontoDeEncontro = "Ponto 3" }
            }
        };
        var resultado = await service.AtualizarLogisticaAsync(usuario.Id, dto2); // substitui por 1

        Assert.True(resultado.Sucesso);
        // ExecuteDelete apagou os 2 antigos; resta só 1
        Assert.Equal(1, await context.Set<LocalVenda>().CountAsync(l => l.VendedorId == vendedor.Id));

        await tx.RollbackAsync();
    }

    // ---------- AtualizarInfo ----------
    [Fact]
    public async Task AtualizarInfo_QuandoSemPerfil_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var service = new VendedorService(context, _imagem);

        var resultado = await service.AtualizarInfoAsync(
            Guid.NewGuid(),
            new AtualizarInfoVendedorDto(null, null, "nova descrição", null));

        Assert.False(resultado.Sucesso);
        Assert.Equal("Usuário não possui um perfil de vendedor válido.", resultado.Mensagem);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task AtualizarInfo_QuandoFotoUrlNaoPertenceAoProvedor_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuario) = await ComVendedorSetup(context);

        // reconfigura o mock: a URL enviada NÃO pertence ao provedor
        _imagem.UrlPertenceAoProvedor("http://malicioso.com/foto.png").Returns(false);

        var resultado = await service.AtualizarInfoAsync(
            usuario.Id,
            new AtualizarInfoVendedorDto("http://malicioso.com/foto.png", null, null, null));

        Assert.False(resultado.Sucesso);
        Assert.Contains("não pertence ao provedor", resultado.Mensagem);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task AtualizarInfo_QuandoChavePixInvalida_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuario) = await ComVendedorSetup(context);

        var resultado = await service.AtualizarInfoAsync(
            usuario.Id,
            new AtualizarInfoVendedorDto(null, null, null, "chave!!invalida"));

        Assert.False(resultado.Sucesso);
        Assert.Contains("chave PIX inválido", resultado.Mensagem);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task AtualizarInfo_QuandoDadosValidos_PersisteDescricaoEChavePix()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuario) = await ComVendedorSetup(context);

        var chavePixValida = $"pix-{Guid.NewGuid():N}@unifesp.br"; // formato e-mail, válido
        var resultado = await service.AtualizarInfoAsync(
            usuario.Id,
            new AtualizarInfoVendedorDto(null, null, "Loja de bolos", chavePixValida));

        Assert.True(resultado.Sucesso);

        var vendedor = await context.Vendedores.AsNoTracking().FirstAsync(v => v.UsuarioId == usuario.Id);
        Assert.Equal("Loja de bolos", vendedor.DescricaoNegocio);
        Assert.Equal(chavePixValida, vendedor.ChavePix);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task AtualizarInfo_QuandoRemoveFoto_SalvaNullEApagaDoProvedor()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var (service, usuario) = await ComVendedorSetup(context);

        // dá uma foto inicial ao vendedor
        var vendedor = await context.Vendedores.FirstAsync(v => v.UsuarioId == usuario.Id);
        vendedor.FotoUrl = "https://cloudinary.com/foto-antiga.png";
        await context.SaveChangesAsync();

        // envia string vazia = pedido de remoção
        var resultado = await service.AtualizarInfoAsync(
            usuario.Id,
            new AtualizarInfoVendedorDto("", null, null, null));

        Assert.True(resultado.Sucesso);

        var atualizado = await context.Vendedores.AsNoTracking().FirstAsync(v => v.UsuarioId == usuario.Id);
        Assert.Null(atualizado.FotoUrl);
        // spy: a imagem antiga deve ter sido apagada do provedor
        await _imagem.Received(1).DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());

        await tx.RollbackAsync();
    }
}
