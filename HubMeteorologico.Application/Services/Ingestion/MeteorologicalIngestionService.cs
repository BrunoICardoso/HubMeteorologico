using HubMeteorologico.Domain.DTOs.Ingestion;
using HubMeteorologico.Domain.Interfaces;
using HubMeteorologico.Infrastructure.Repository.Interface;
using Microsoft.Extensions.Logging;

namespace HubMeteorologico.Domain.Services.Ingestion;

public class MeteorologicalIngestionService : IMeteorologicalIngestionService
{
    private readonly IRegistrosMeteorologicosRepository _repository;
    private readonly IExternalMeteorologicalProvider _provider;
    private readonly IInterpolationQueue _interpolationQueue;
    private readonly ILogger<MeteorologicalIngestionService> _logger;

    public MeteorologicalIngestionService(
        IRegistrosMeteorologicosRepository repository,
        IExternalMeteorologicalProvider provider,
        IInterpolationQueue interpolationQueue,
        ILogger<MeteorologicalIngestionService> logger)
    {
        _repository = repository;
        _provider = provider;
        _interpolationQueue = interpolationQueue;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<InterpolationRequestedMessage>> ImportFullHourAsync(
        DateTime dataHora,
        CancellationToken cancellationToken = default)
    {
        var fullHour = NormalizeFullHour(dataHora);
        var anoAgricolaId = await _repository.GetAnoAgricolaIdAsync(fullHour, cancellationToken);

        if (anoAgricolaId is null)
        {
            _logger.LogWarning("Nenhum ano agrícola encontrado para {DataHora}. Ingestão ignorada.", fullHour);
            return Array.Empty<InterpolationRequestedMessage>();
        }

        var equipamentos = await _repository.GetActiveEquipamentosAsync(cancellationToken);
        if (equipamentos.Count == 0)
        {
            _logger.LogInformation("Nenhum equipamento ativo encontrado para ingestão em {DataHora}.", fullHour);
            return Array.Empty<InterpolationRequestedMessage>();
        }

        var readings = await _provider.FetchReadingsAsync(equipamentos, fullHour, anoAgricolaId.Value, cancellationToken);
        var imported = await _repository.UpsertBatchAsync(readings, cancellationToken);

        var messages = readings
            .GroupBy(r => new { r.FazendaId, r.DataHora })
            .Select(g => new InterpolationRequestedMessage
            {
                FazendaId = g.Key.FazendaId,
                DataHora = g.Key.DataHora,
                ImportedReadings = g.Count()
            })
            .ToArray();

        foreach (var message in messages)
            await _interpolationQueue.EnqueueAsync(message, cancellationToken);

        _logger.LogInformation(
            "Ingestão finalizada para {DataHora}: {Imported} leituras persistidas e {Messages} mensagens de interpolação enfileiradas.",
            fullHour,
            imported,
            messages.Length);

        return messages;
    }

    private static DateTime NormalizeFullHour(DateTime dataHora)
        => new(dataHora.Year, dataHora.Month, dataHora.Day, dataHora.Hour, 0, 0, dataHora.Kind);
}
