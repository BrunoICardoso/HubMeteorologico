using HubMeteorologico.Domain.Appsettings;
using HubMeteorologico.Domain.Interfaces;
using Microsoft.Extensions.Options;

namespace HubMeteorologico.API.Workers;

public class MeteorologicalIngestionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptionsMonitor<IngestionWorkerOptions> _options;
    private readonly ILogger<MeteorologicalIngestionWorker> _logger;

    public MeteorologicalIngestionWorker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<IngestionWorkerOptions> options,
        ILogger<MeteorologicalIngestionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker de ingestão meteorológica iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = _options.CurrentValue;
            var interval = TimeSpan.FromMinutes(Math.Max(1, options.IntervalMinutes));

            try
            {
                await RunOnceAsync(options, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar ciclo do worker de ingestão meteorológica.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task RunOnceAsync(IngestionWorkerOptions options, CancellationToken cancellationToken)
    {
        var reference = DateTime.UtcNow.AddHours(-Math.Max(1, options.LookbackHours));
        var fullHour = new DateTime(reference.Year, reference.Month, reference.Day, reference.Hour, 0, 0, DateTimeKind.Utc);

        using var scope = _scopeFactory.CreateScope();
        var ingestionService = scope.ServiceProvider.GetRequiredService<IMeteorologicalIngestionService>();

        _logger.LogInformation("Iniciando ingestão de dados meteorológicos externos para {DataHora}.", fullHour);
        await ingestionService.ImportFullHourAsync(fullHour, cancellationToken);
    }
}
