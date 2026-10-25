using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs;
using UniHub.Application.Services;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;
using Xunit;

namespace UniHub.Tests.Integracao;

/// <summary>
/// Testes de integração do VoluntarioService contra a branch development do Neon.
///
/// Por que integração e não unidade: o service depende diretamente do AppDbContext
/// (consultas EF, Include, SaveChanges), não de uma interface mockável. Testá-lo
/// com banco real cobre a lógica das consultas de verdade.
///
/// Cada teste roda dentro de uma transação com ROLLBACK no fim (ver
/// IntegracaoTestBase), então nada fica persistido e os testes são repetíveis.
///
/// FORA DE ESCOPO aqui: o ramo catch (DbUpdateException) de corrida no
/// TornarSeVoluntarioAsync — ele depende de duas conexões/transações simultâneas
/// disputando o índice único em Voluntarios.UsuarioId, o que não é reproduzível
/// dentro de uma única transação. Fica como pendência de um teste de concorrência
/// dedicado.
/// </summary>
public class VoluntarioServiceIntegracaoTests : IntegracaoTestBase
{
    private static Usuario NovoUsuario(bool ativo = true) => new()
    {
        NomeCompleto = "Usuario Teste",
        EmailInstitucional = $"teste-{Guid.NewGuid():N}@unifesp.br",
        IsActive = ativo
    };

    // ---------- TornarSeVoluntario ----------
    [Fact]
    public async Task TornarSeVoluntario_QuandoUsuarioNaoExiste_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();
        var service = new VoluntarioService(context);

        var resultado = await service.TornarSeVoluntarioAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Equal("Usuário não encontrado.", resultado.Mensagem);
        Assert.Null(resultado.Voluntario);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task TornarSeVoluntario_QuandoContaSuspensa_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var usuario = NovoUsuario(ativo: false);
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var service = new VoluntarioService(context);
        var resultado = await service.TornarSeVoluntarioAsync(usuario.Id);

        Assert.False(resultado.Sucesso);
        Assert.Equal("Conta suspensa. Procure a administração.", resultado.Mensagem);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task TornarSeVoluntario_QuandoUsuarioValido_CriaPerfilERetornaSucesso()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var usuario = NovoUsuario();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var service = new VoluntarioService(context);
        var resultado = await service.TornarSeVoluntarioAsync(usuario.Id);

        Assert.True(resultado.Sucesso);
        Assert.Equal("Perfil de voluntário criado com sucesso!", resultado.Mensagem);
        Assert.NotNull(resultado.Voluntario);
        Assert.True(resultado.Voluntario!.Disponivel);          // nasce disponivel
        Assert.StartsWith("VOL-", resultado.Voluntario.CodigoPublico);
        Assert.Empty(resultado.Voluntario.AreasDeAtuacao);

        // o perfil foi de fato persistido (dentro da transacao)
        Assert.True(await context.Voluntarios.AnyAsync(v => v.UsuarioId == usuario.Id));

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task TornarSeVoluntario_QuandoJaTemPerfil_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var usuario = NovoUsuario();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var service = new VoluntarioService(context);
        await service.TornarSeVoluntarioAsync(usuario.Id); // primeira vez: cria

        var resultado = await service.TornarSeVoluntarioAsync(usuario.Id); // segunda vez

        Assert.False(resultado.Sucesso);
        Assert.Equal("Usuário já possui um perfil de voluntário.", resultado.Mensagem);
        // mesmo na falha, devolve o DTO do perfil existente
        Assert.NotNull(resultado.Voluntario);

        await tx.RollbackAsync();
    }

    // ---------- ObterMeuPerfil ----------
    [Fact]
    public async Task ObterMeuPerfil_QuandoNaoTemPerfil_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var service = new VoluntarioService(context);
        var resultado = await service.ObterMeuPerfilAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Equal("Usuário não possui um perfil de voluntário.", resultado.Mensagem);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task ObterMeuPerfil_QuandoTemPerfil_RetornaPerfil()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var usuario = NovoUsuario();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        var service = new VoluntarioService(context);
        await service.TornarSeVoluntarioAsync(usuario.Id);

        var resultado = await service.ObterMeuPerfilAsync(usuario.Id);

        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Voluntario);

        await tx.RollbackAsync();
    }

    // ---------- DefinirAreas ----------
    [Fact]
    public async Task DefinirAreas_QuandoListaVazia_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var service = new VoluntarioService(context);
        var resultado = await service.DefinirAreasAsync(Guid.NewGuid(), new DefinirAreasDto(new List<CategoriaApoio>()));

        Assert.False(resultado.Sucesso);
        Assert.Equal("Informe ao menos uma área de atuação.", resultado.Mensagem);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task DefinirAreas_QuandoCategoriaInvalida_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var service = new VoluntarioService(context);
        // 99 nao e um valor definido do enum CategoriaApoio
        var areasInvalidas = new List<CategoriaApoio> { (CategoriaApoio)99 };
        var resultado = await service.DefinirAreasAsync(Guid.NewGuid(), new DefinirAreasDto(areasInvalidas));

        Assert.False(resultado.Sucesso);
        Assert.Equal("Área de atuação inválida.", resultado.Mensagem);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task DefinirAreas_QuandoSemPerfil_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var service = new VoluntarioService(context);
        var areas = new List<CategoriaApoio> { CategoriaApoio.AulasDeReforco };
        var resultado = await service.DefinirAreasAsync(Guid.NewGuid(), new DefinirAreasDto(areas));

        Assert.False(resultado.Sucesso);
        Assert.Equal("Usuário não possui um perfil de voluntário.", resultado.Mensagem);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task DefinirAreas_QuandoValido_SalvaAreasOrdenadasESemDuplicatas()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var usuario = NovoUsuario();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        var service = new VoluntarioService(context);
        await service.TornarSeVoluntarioAsync(usuario.Id);

        // fora de ordem e com duplicata -> service aplica Distinct().OrderBy()
        var areas = new List<CategoriaApoio>
        {
            CategoriaApoio.MaterialAcademico,
            CategoriaApoio.AulasDeReforco,
            CategoriaApoio.AulasDeReforco
        };
        var resultado = await service.DefinirAreasAsync(usuario.Id, new DefinirAreasDto(areas));

        Assert.True(resultado.Sucesso);
        Assert.Equal(
            new[] { CategoriaApoio.AulasDeReforco, CategoriaApoio.MaterialAcademico },
            resultado.Voluntario!.AreasDeAtuacao);

        await tx.RollbackAsync();
    }

    // ---------- Pausar / Retomar ----------
    [Fact]
    public async Task PausarAtuacao_QuandoTemPerfil_DeixaIndisponivel()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var usuario = NovoUsuario();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        var service = new VoluntarioService(context);
        await service.TornarSeVoluntarioAsync(usuario.Id);

        var resultado = await service.PausarAtuacaoAsync(usuario.Id);

        Assert.True(resultado.Sucesso);
        Assert.False(resultado.Voluntario!.Disponivel);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task RetomarAtuacao_QuandoPausado_DeixaDisponivelNovamente()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var usuario = NovoUsuario();
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
        var service = new VoluntarioService(context);
        await service.TornarSeVoluntarioAsync(usuario.Id);
        await service.PausarAtuacaoAsync(usuario.Id); // pausa antes

        var resultado = await service.RetomarAtuacaoAsync(usuario.Id);

        Assert.True(resultado.Sucesso);
        Assert.True(resultado.Voluntario!.Disponivel);

        await tx.RollbackAsync();
    }

    [Fact]
    public async Task PausarAtuacao_QuandoSemPerfil_RetornaFalha()
    {
        await using var context = CriarContexto();
        await using var tx = await context.Database.BeginTransactionAsync();

        var service = new VoluntarioService(context);
        var resultado = await service.PausarAtuacaoAsync(Guid.NewGuid());

        Assert.False(resultado.Sucesso);
        Assert.Equal("Usuário não possui um perfil de voluntário.", resultado.Mensagem);

        await tx.RollbackAsync();
    }
}
