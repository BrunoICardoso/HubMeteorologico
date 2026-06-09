using HubMeteorologico.Domain.DTOs.Ingestion;
using HubMeteorologico.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HubMeteorologico.Domain.Services.Ingestion;

public class InterpolationService : IInterpolationService
{
    private readonly ILogger<InterpolationService> _logger;

    public InterpolationService(ILogger<InterpolationService> logger)
    {
        _logger = logger;
    }

    public Task ProcessAsync(InterpolationRequestedMessage message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Processando interpolação da fazenda {FazendaId} na hora {DataHora}. Leituras importadas: {ImportedReadings}. MessageId: {MessageId}",
            message.FazendaId,
            message.DataHora,
            message.ImportedReadings,
            message.MessageId);

        return Task.CompletedTask;
    }
}
