
using HubMeteorologico.Domain.Entities;
using HubMeteorologico.Infrastructure.Repository.Settings.Interface;

namespace HubMeteorologico.Domain.Interfaces.Services;

public interface IFazendaRepository : IRepository<Fazendas>
{
    Task<bool> LavouraExistsInFazendaAsync(int fazendaId, string codigoLavoura);
}