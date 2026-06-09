using HubMeteorologico.Domain.Interfaces;

namespace HubMeteorologico.API.Workers;

public class InterpolationWorker : BackgroundService
{
    private readonly IInterpolationQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InterpolationWorker> _logger;

    public InterpolationWorker(
        IInterpolationQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<InterpolationWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker de interpolação iniciado e aguardando mensagens.");

        await foreach (var message in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var interpolationService = scope.ServiceProvider.GetRequiredService<IInterpolationService>();
                await interpolationService.ProcessAsync(message, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao processar mensagem de interpolação {MessageId} da fazenda {FazendaId} em {DataHora}.",
                    message.MessageId,
                    message.FazendaId,
                    message.DataHora);
            }
        }
    }
}
