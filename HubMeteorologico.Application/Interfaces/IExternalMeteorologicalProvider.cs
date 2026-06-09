using HubMeteorologico.Domain.DTOs.Ingestion;

namespace HubMeteorologico.Domain.Interfaces;

public interface IExternalMeteorologicalProvider
{
    Task<IReadOnlyCollection<ExternalMeteorologicalReadingDto>> FetchReadingsAsync(
        IReadOnlyCollection<EquipamentoIngestionDto> equipamentos,
        DateTime dataHora,
        int anoAgricolaId,
        CancellationToken cancellationToken = default);
}
