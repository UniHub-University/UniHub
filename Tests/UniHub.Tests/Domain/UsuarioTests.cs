using System;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Domain;

public class UsuarioTests
{
    // ---------- SuspenderConta / ReativarConta ----------
    [Fact]
    public void SuspenderConta_DeixaContaInativa()
    {
        var usuario = new Usuario(); // nasce IsActive = true
        usuario.SuspenderConta();
        Assert.False(usuario.IsActive);
    }

    [Fact]
    public void ReativarConta_DeixaContaAtivaNovamente()
    {
        var usuario = new Usuario();
        usuario.SuspenderConta();  // primeiro suspende (senão passaria "por sorte")
        usuario.ReativarConta();   // depois reativa
        Assert.True(usuario.IsActive);
    }

    // ---------- TornarSeVendedor ----------
    [Fact]
    public void TornarSeVendedor_QuandoNaoTemPerfil_CriaEAssociaVendedor()
    {
        var usuario = new Usuario();

        var vendedor = usuario.TornarSeVendedor();

        Assert.NotNull(vendedor);              // retorna o perfil criado
        Assert.NotNull(usuario.Vendedor);      // e associa ao usuario
        Assert.Same(vendedor, usuario.Vendedor); // é o mesmo objeto
        Assert.Equal(usuario.Id, vendedor.UsuarioId);
    }

    [Fact]
    public void TornarSeVendedor_QuandoJaTemPerfil_LancaExcecao()
    {
        var usuario = new Usuario();
        usuario.TornarSeVendedor(); // já passa a ter perfil

        Assert.Throws<InvalidOperationException>(() => usuario.TornarSeVendedor());
    }

    // ---------- TornarSeVoluntario ----------
    [Fact]
    public void TornarSeVoluntario_QuandoNaoTemPerfil_CriaEAssociaVoluntario()
    {
        var usuario = new Usuario();

        var voluntario = usuario.TornarSeVoluntario();

        Assert.NotNull(voluntario);
        Assert.NotNull(usuario.Voluntario);
        Assert.Same(voluntario, usuario.Voluntario);
        Assert.Equal(usuario.Id, voluntario.UsuarioId);
    }

    [Fact]
    public void TornarSeVoluntario_QuandoJaTemPerfil_LancaExcecao()
    {
        var usuario = new Usuario();
        usuario.TornarSeVoluntario();

        Assert.Throws<InvalidOperationException>(() => usuario.TornarSeVoluntario());
    }
}
