using System;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

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

    public async Task<Usuario> ValidarLoginGoogleAsync(string idToken)
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

        // Retorna o usuário validado (futuramente, conectaremos com o banco aqui)
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
        // Retorno obrigatório do usuário (novo ou existente)
        return usuario;
    }
}