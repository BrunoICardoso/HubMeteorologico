using HubMeteorologico.Domain.DTOs.Ingestion;

namespace HubMeteorologico.Domain.Interfaces;

public interface IInterpolationQueue
{
    ValueTask EnqueueAsync(InterpolationRequestedMessage message, CancellationToken cancellationToken = default);

    IAsyncEnumerable<InterpolationRequestedMessage> DequeueAllAsync(CancellationToken cancellationToken = default);
}
