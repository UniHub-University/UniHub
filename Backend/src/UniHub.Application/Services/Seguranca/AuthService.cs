using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Application.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<Usuario> ValidarLoginGoogleAsync(string idToken)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);

        if (!payload.Email.EndsWith("@unifesp.br"))
        {
            throw new UnauthorizedAccessException("Acesso negado. Utilize seu e-mail @unifesp.br.");
        }

        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.EmailInstitucional == payload.Email);

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
        
        return usuario;
    }

    // NOVO MÉTODO: Cria o token oficial do UniHub usando os dados do appsettings.json
    public string GerarTokenJwt(Usuario usuario)
    {
        var jwtSettings = _config.GetSection("JwtSettings");
        var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecureKey"]!);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.EmailInstitucional),
            new Claim(JwtRegisteredClaimNames.Name, usuario.NomeCompleto)
        };

        var key = new SymmetricSecurityKey(secretKey);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(4), // Token válido por 4 horas
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}