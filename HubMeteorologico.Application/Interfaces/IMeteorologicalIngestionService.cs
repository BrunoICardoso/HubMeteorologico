using HubMeteorologico.Domain.DTOs.Ingestion;

namespace HubMeteorologico.Domain.Interfaces;

public interface IMeteorologicalIngestionService
{
    Task<IReadOnlyCollection<InterpolationRequestedMessage>> ImportFullHourAsync(
        DateTime dataHora,
        CancellationToken cancellationToken = default);
}
