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
using UniHub.Application.Interfaces.Alimentacao;
using UniHub.Application.Services.Alimentacao;
using UniHub.Infrastructure.Repositories.Alimentacao;
using UniHub.API.BackgroundServices;
using UniHub.API.Hubs;

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

// Frente 2 (BRV-185): SignalR para o Painel do Vendedor em tempo real.
// O hub (PainelHub) agrupa cada conexao em "vendedor:{id}"; a emissao de
// eventos chega nas tasks A3/A4. A leitura do JWT via access_token na query
// string e o AllowCredentials() no CORS vem na task A2 (PR isolado).
builder.Services.AddSignalR();

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
builder.Services.AddScoped<VoluntarioService>();

builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IVendedorConsultaRepository, VendedorConsultaRepository>();
builder.Services.AddScoped<IProdutoBuscaRepository, ProdutoBuscaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<IVendedorPublicoRepository, VendedorPublicoRepository>();
builder.Services.AddScoped<VendedorPublicoService>();
builder.Services.AddScoped<ISolicitacaoApoioRepository, SolicitacaoApoioRepository>();
builder.Services.AddScoped<IAlimentoRepository, AlimentoRepository>();
builder.Services.AddScoped<AlimentoService>();

builder.Services.AddScoped<IDenunciaRepository, DenunciaRepository>();

// P4: job de expiracao de reservas (devolve estoque + marca Expirado a cada 10 min).
builder.Services.AddHostedService<ExpiracaoReservasBackgroundService>();



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

    // SignalR (BRV-185/196): o handshake por WebSocket nao envia o header
    // "Authorization" -- o cliente manda o JWT na query string (?access_token=...).
    // Copiamos esse valor para o contexto APENAS quando a requisicao e para um
    // hub (path iniciando em "/hubs"), para nao abrir um segundo canal de
    // autenticacao nas rotas REST (essas continuam so pelo header).
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
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
            // SignalR autenticado exige AllowCredentials(), que e INCOMPATIVEL
            // com AllowAnyOrigin(). Em Development liberamos qualquer origem via
            // SetIsOriginAllowed (equivalente ao "any", porem compativel com
            // credenciais) para manter o fluxo de dev sem travar o hub.
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
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

            // WithOrigins (lista fechada) + AllowCredentials: combinacao exigida
            // pelo SignalR autenticado. Em producao a origem e a do front oficial.
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
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

// Endpoint do hub do Painel do Vendedor (Frente 2 / BRV-185). O path
// "/hubs/painel" e o combinado no contrato dos eventos (T0) com a trilha
// do front. O [Authorize] no PainelHub exige JWT valido para conectar.
app.MapHub<PainelHub>("/hubs/painel");

// Health check leve e publico. Serve a dois propositos:
// 1. Render usa para saber se o servico subiu com sucesso.
// 2. Pode ser pingado por um cron (ex: cron-job.org) a cada ~10min para evitar
//    o spin down do plano free (15min de inatividade) e para "aquecer" o Neon
//    antes do teste de concorrencia, mitigando o cold start.
app.MapGet("/health", () => Results.Ok(new { status = "ok", time = DateTime.UtcNow }))
   .AllowAnonymous();

app.Run();
