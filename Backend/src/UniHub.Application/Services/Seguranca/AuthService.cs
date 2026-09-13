using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;

using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;
using UniHub.Application.DTOs.Seguranca;

namespace UniHub.Application.Services;

public class AuthService
{
    // É a conexão com o banco de dados (Neon/Postgres, via Entity Framework).
    // Sem ele, o AuthService não teria como consultar se um usuário já existe
    // (_context.Usuarios.FirstOrDefaultAsync) nem salvar um novo usuário
    // no primeiro acesso (_context.Usuarios.Add / SaveChangesAsync).
    private readonly AppDbContext _context;

    // É o acesso às configurações do appsettings.json — em especial,
    // a chave secreta usada para assinar o Token JWT (JwtSettings:SecureKey).
    // Sem ele, o método que gera o token não teria de onde ler essa chave.
    private readonly IConfiguration _config;

    // O contexto do banco e a configuração (chaves do JWT) chegam via
    // injeção de dependência, em vez de serem instanciados aqui dentro.
    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<LoginResult> ValidarLoginGoogleAsync(string idToken)
    {
        // Valida a assinatura do token diretamente com os servidores do Google
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

        // Bloqueia o acesso de qualquer e-mail que não seja da instituição
        if (!payload.Email.EndsWith("@unifesp.br"))
        {
            throw new UnauthorizedAccessException("Acesso negado. Utilize seu e-mail @unifesp.br.");
        }

        // Consulta se esse usuário já existe no banco pelo email institucional
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.EmailInstitucional == payload.Email);

        // Se não existir, cria e persiste o usuário (primeiro acesso)
        if (usuario == null)
        {
            usuario = new Usuario
            {
                EmailInstitucional = payload.Email,
                NomeCompleto = payload.Name,
                GoogleId = payload.Subject,
                FotoPerfilUrl = payload.Picture ?? string.Empty
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
        }
        // Gera o token JWT e empacota junto com os dados do usuário
        var token = GerarTokenJwt(usuario);
        return new LoginResult(
            token, 
            usuario.Id, 
            usuario.NomeCompleto, 
            usuario.EmailInstitucional, 
            usuario.FotoPerfilUrl
        );
    }


    // Gera um JWT (JSON Web Token) assinado, contendo as informações essenciais do usuário autenticado
    // É esse token que o frontend vai guardar após o login e reenviar em
    // todas as requisições futuras (no cabeçalho "Authorization: Bearer ..."),
    // pra provar quem é o usuário sem precisar logar de novo a cada clique.
    //
    // "private" porque é um detalhe interno de COMO o AuthService cumpre sua responsabilidade
    private string GerarTokenJwt(Usuario usuario)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // Lê a chave secreta do appsettings.json (seção "JwtSettings"),
        // a mesma que vocês já configuraram juntos.
        var chaveSecreta = _config["JwtSettings:SecureKey"]
            ?? throw new InvalidOperationException("JwtSettings:SecureKey não configurada.");
        var key = Encoding.UTF8.GetBytes(chaveSecreta);

        // Tempo de expiração configurável via appsettings.json.
        // Se não vier configurado, usa 120 minutos como padrão.
        var minutosExpiracao = _config.GetValue<int?>("JwtSettings:MinutosExpiracao") ?? 120;

        // Cria as regras e os dados do token (Claims)
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.EmailInstitucional),
                new Claim(ClaimTypes.Name, usuario.NomeCompleto),
                new Claim(ClaimTypes.Role, usuario.Role.ToString())
            }),
            Expires = DateTime.UtcNow.AddMinutes(minutosExpiracao),
            Issuer = _config["JwtSettings:Issuer"],
            Audience = _config["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}