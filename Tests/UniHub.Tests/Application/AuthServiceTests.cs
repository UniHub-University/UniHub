using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using Microsoft.Extensions.Configuration;
using UniHub.Application.Services;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Application;

/// <summary>
/// Testes de unidade do AuthService — SOMENTE o GerarTokenJwt.
///
/// GerarTokenJwt é puro: recebe um Usuario + IConfiguration e devolve a string
/// do JWT, sem tocar banco nem Google. Montamos um IConfiguration em memória
/// (chave de teste, nunca um segredo real) e lemos o token de volta para
/// inspecionar os claims.
///
/// FORA DE ESCOPO (débito registrado): ValidarLoginGoogleAsync não é testado
/// aqui porque chama GoogleJsonWebSignature.ValidateAsync (método estático da
/// lib do Google) diretamente — não há seam para substituir o Google por um
/// dublê. Testá-lo exigiria refatorar o service para depender de uma interface
/// (ex.: IGoogleTokenValidator), o que é mudança de código de produção no fluxo
/// de autenticação e deve passar por alinhamento/PR próprio com o squad.
/// </summary>
public class AuthServiceTests
{
    // Chave de teste (>= 32 bytes, exigência do HmacSha256). NÃO é um segredo real.
    private const string ChaveDeTeste = "chave-de-teste-super-secreta-com-32+-caracteres";

    private static AuthService CriarService(string? minutosExpiracao = null)
    {
        var valores = new Dictionary<string, string?>
        {
            ["JwtSettings:SecureKey"] = ChaveDeTeste,
            ["JwtSettings:Issuer"] = "unihub-teste",
            ["JwtSettings:Audience"] = "unihub-clientes",
        };
        if (minutosExpiracao is not null)
            valores["JwtSettings:MinutosExpiracao"] = minutosExpiracao;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(valores)
            .Build();

        // AppDbContext não é usado por GerarTokenJwt; passamos null com segurança
        // porque o método não o referencia.
        return new AuthService(null!, config);
    }

    private static Usuario NovoUsuario() => new()
    {
        Id = Guid.NewGuid(),
        NomeCompleto = "Maria Silva",
        EmailInstitucional = "maria@unifesp.br",
        Role = UserRole.Aluno
    };

    private static JwtSecurityToken Ler(string token) =>
        new JwtSecurityTokenHandler().ReadJwtToken(token);

    // ---------- Claim "sub" (regra crítica do projeto) ----------
    [Fact]
    public void GerarTokenJwt_ColocaIdDoUsuarioNoClaimSub()
    {
        var service = CriarService();
        var usuario = NovoUsuario();

        var token = service.GerarTokenJwt(usuario);
        var lido = Ler(token);

        // o projeto extrai o usuário autenticado via claim "sub" -> este é o claim
        // que importa (AGENTS.md: erro nº 1 é usar o claim errado)
        var sub = lido.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value;
        Assert.Equal(usuario.Id.ToString(), sub);
    }

    // ---------- Demais claims ----------
    [Fact]
    public void GerarTokenJwt_IncluiEmailNomeERole()
    {
        var service = CriarService();
        var usuario = NovoUsuario();

        var lido = Ler(service.GerarTokenJwt(usuario));

        Assert.Equal(usuario.EmailInstitucional, lido.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(usuario.NomeCompleto, lido.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.Equal(usuario.Role.ToString(), lido.Claims.First(c => c.Type == "role").Value);
    }

    // ---------- Issuer / Audience ----------
    [Fact]
    public void GerarTokenJwt_UsaIssuerEAudienceDaConfiguracao()
    {
        var service = CriarService();

        var lido = Ler(service.GerarTokenJwt(NovoUsuario()));

        Assert.Equal("unihub-teste", lido.Issuer);
        Assert.Contains("unihub-clientes", lido.Audiences);
    }

    // ---------- Expiração ----------
    [Fact]
    public void GerarTokenJwt_QuandoMinutosNaoConfigurado_UsaPadraoDe120Minutos()
    {
        var service = CriarService(minutosExpiracao: null);

        var antes = DateTime.UtcNow;
        var lido = Ler(service.GerarTokenJwt(NovoUsuario()));

        // ~120 min de validade; margem para o tempo de execução do teste
        var minutos = (lido.ValidTo - antes).TotalMinutes;
        Assert.InRange(minutos, 118, 121);
    }

    [Fact]
    public void GerarTokenJwt_QuandoMinutosConfigurado_RespeitaOValor()
    {
        var service = CriarService(minutosExpiracao: "30");

        var antes = DateTime.UtcNow;
        var lido = Ler(service.GerarTokenJwt(NovoUsuario()));

        var minutos = (lido.ValidTo - antes).TotalMinutes;
        Assert.InRange(minutos, 28, 31);
    }
}
