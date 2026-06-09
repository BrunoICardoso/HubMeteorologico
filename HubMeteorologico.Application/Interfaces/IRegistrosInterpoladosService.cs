using HubMeteorologico.Domain.DTOs.RegistrosInterpolados;
using HubMeteorologico.Domain.ResponseDefault;

namespace HubMeteorologico.Domain.Interfaces;

public interface IRegistrosInterpoladosService
{
    Task<ReturnDefault<IReadOnlyCollection<RegistrosInterpoladosDto>>> GetAsync(RegistrosInterpoladosFilterDto filter, CancellationToken cancellationToken = default);
}
