using Microsoft.EntityFrameworkCore;
using UniHub.Infrastructure.Data;
using UniHub.Application.Services;
using UniHub.Application.DTOs;
using UniHub.Application.Interfaces;
using UniHub.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Application.Services.Vendas;
using UniHub.Infrastructure.Repositories.Vendas;
using UniHub.Infrastructure.Repositories;
using UniHub.Application.Interfaces.AcaoSolidaria;
using UniHub.Infrastructure.Repositories.AcaoSolidaria;

var builder = WebApplication.CreateBuilder(args);

// Render (e containers em geral) definem a porta de escuta via variavel PORT
// e esperam a app escutando em 0.0.0.0. Localmente PORT nao existe, entao o
// launchSettings continua valendo e o fluxo de desenvolvimento nao muda.
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllers();

builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("Cloudinary"));

builder.Services.AddSingleton<IImageStorageService, CloudinaryImageStorageService>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));

    // EnableSensitiveDataLogging expoe valores reais dos parametros (inclusive
    // dados pessoais) no log -- util para depurar localmente, mas um risco de
    // vazamento (e de violacao da LGPD) em producao. Fica restrito a Development.
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging()
               .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VendedorService>();

builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<ISolicitacaoApoioRepository, SolicitacaoApoioRepository>();

// --- INÍCIO DA CONFIGURAÇÃO JWT ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecureKey"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "role"
    };
});
// --- FIM DA CONFIGURAÇÃO JWT ---

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        // Verifica se o sistema está rodando em ambiente local (Development)
        if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // Em producao, trava a API apenas para o(s) dominio(s) oficial(is) do
            // frontend. As origens vem da configuracao "Cors:AllowedOrigins"
            // (definida via variavel de ambiente Cors__AllowedOrigins__0,
            // Cors__AllowedOrigins__1, ...), com fallback para os dominios de
            // exemplo caso nada seja configurado.
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>();

            if (allowedOrigins is null || allowedOrigins.Length == 0)
            {
                allowedOrigins = ["https://unihub.com.br", "https://unihub-app.vercel.app"];
            }

            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

var app = builder.Build();

// Swagger habilitado tambem em producao para validacao/testes online da API
// durante a Sprint 3 (frontend ainda nao desenvolvido). O time testa via
// Swagger + "Authorize" com token obtido no Google OAuth Playground.
// TODO: considerar restringir/desligar apos a entrega, quando o front assumir.
app.UseSwagger();
app.UseSwaggerUI();


app.UseCors("PermitirFrontend");

// A ordem aqui é CRÍTICA. Authentication sempre antes de Authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Health check leve e publico. Serve a dois propositos:
// 1. Render usa para saber se o servico subiu com sucesso.
// 2. Pode ser pingado por um cron (ex: cron-job.org) a cada ~10min para evitar
//    o spin down do plano free (15min de inatividade) e para "aquecer" o Neon
//    antes do teste de concorrencia, mitigando o cold start.
app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }))
   .AllowAnonymous();

app.Run();
