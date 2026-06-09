using HubMeteorologico.Domain.DTOs.Ingestion;
using HubMeteorologico.Domain.Entities;
using HubMeteorologico.Infrastructure.Repository.Settings.Interface;

namespace HubMeteorologico.Infrastructure.Repository.Interface;

public interface IRegistrosMeteorologicosRepository : IRepository<RegistrosMeteorologicos>
{
    Task<IReadOnlyCollection<EquipamentoIngestionDto>> GetActiveEquipamentosAsync(
        CancellationToken cancellationToken = default);

    Task<int?> GetAnoAgricolaIdAsync(DateTime dataHora, CancellationToken cancellationToken = default);

    Task<int> UpsertBatchAsync(
        IReadOnlyCollection<ExternalMeteorologicalReadingDto> readings,
        CancellationToken cancellationToken = default);
}
