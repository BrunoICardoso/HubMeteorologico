using HubMeteorologico.Domain.DTOs.Ingestion;

namespace HubMeteorologico.Domain.Interfaces;

public interface IInterpolationService
{
    Task ProcessAsync(InterpolationRequestedMessage message, CancellationToken cancellationToken = default);
}
