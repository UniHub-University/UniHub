using UniHub.Application.Services.Vendas;

namespace UniHub.API.BackgroundServices;

/// <summary>
/// P4: job de expiracao de reservas. A cada 10 minutos, varre as reservas
/// vencidas, devolve o estoque e marca os pedidos como Expirado (a logica fica
/// no PedidoService, reusada tambem pela checagem lazy do P5).
///
/// Detalhe critico de DI: um BackgroundService e SINGLETON, mas o PedidoService
/// (e o AppDbContext que ele usa) e SCOPED. Por isso resolvemos o PedidoService
/// dentro de um escopo criado a cada ciclo via IServiceScopeFactory -- injetar
/// o service scoped direto no construtor quebraria em runtime.
/// </summary>
public class ExpiracaoReservasBackgroundService : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiracaoReservasBackgroundService> _logger;

    public ExpiracaoReservasBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiracaoReservasBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Intervalo);

        // Roda um ciclo ao subir e depois a cada Intervalo, ate o app encerrar.
        do
        {
            await ExpirarAsync(stoppingToken);
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ExpirarAsync(CancellationToken stoppingToken)
    {
        // Um escopo novo por ciclo: resolve o PedidoService (scoped) com um
        // AppDbContext proprio, sem vazar contexto entre ciclos.
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var pedidoService = scope.ServiceProvider.GetRequiredService<PedidoService>();

            var expiradas = await pedidoService.ExpirarReservasVencidasAsync();
            if (expiradas > 0)
                _logger.LogInformation("Expiracao de reservas: {Qtd} reserva(s) expirada(s).", expiradas);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Encerramento normal da aplicacao -- nao e erro.
        }
        catch (Exception ex)
        {
            // Um ciclo que falha (ex.: banco indisponivel) nao pode derrubar o job:
            // loga e tenta de novo no proximo tick.
            _logger.LogError(ex, "Falha no ciclo de expiracao de reservas.");
        }
    }
}
