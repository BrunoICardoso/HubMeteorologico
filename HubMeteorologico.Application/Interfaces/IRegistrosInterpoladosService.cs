using HubMeteorologico.Domain.DTOs.RegistrosInterpolados;
using HubMeteorologico.Domain.ResponseDefault;

namespace HubMeteorologico.Domain.Interfaces;

public interface IRegistrosInterpoladosService
{
    Task<ReturnDefault<IEnumerable<RegistrosInterpoladosDto>>> GetAsync(RegistrosInterpoladosFilterDto filter);
}