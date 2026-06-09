using HubMeteorologico.Domain.DTOs.RegistrosInterpolados;
using HubMeteorologico.Domain.Entities;
using HubMeteorologico.Infrastructure.Repository.Settings.Interface;

namespace HubMeteorologico.Infrastructure.Repository.Interface;

public interface IRegistrosInterpoladosRepository: IRepository<RegistrosInterpolados>
{
    Task<IEnumerable<RegistrosInterpoladosDto>> GetByFilterAsync(RegistrosInterpoladosFilterDto filter, CancellationToken cancellationToken = default);
}
