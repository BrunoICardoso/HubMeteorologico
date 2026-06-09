using System.Net;
using System.Text.Json;
using HubMeteorologico.Domain.DTOs.RegistrosInterpolados;
using HubMeteorologico.Domain.Interfaces;
using HubMeteorologico.Domain.Interfaces.Services;
using HubMeteorologico.Domain.ResponseDefault;
using HubMeteorologico.Infrastructure.Repository.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace HubMeteorologico.Domain.Services;

public class RegistrosInterpoladosService : IRegistrosInterpoladosService
{
    private readonly IRegistrosInterpoladosRepository _repository;
    private readonly IFazendaRepository _fazendaRepository;
    private readonly IMapaFazendaLavouraRepository _mapaFazendaLavouraRepository;
    private readonly IDistributedCache _cache;
    private readonly ILogger<RegistrosInterpoladosService> _logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public RegistrosInterpoladosService(
        IRegistrosInterpoladosRepository repository,
        IFazendaRepository fazendaRepository,
        IMapaFazendaLavouraRepository mapaFazendaLavouraRepository,
        IDistributedCache cache,
        ILogger<RegistrosInterpoladosService> logger)
    {
        _repository = repository;
        _fazendaRepository = fazendaRepository;
        _mapaFazendaLavouraRepository = mapaFazendaLavouraRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ReturnDefault<IEnumerable<RegistrosInterpoladosDto>>> GetAsync(RegistrosInterpoladosFilterDto filter)
    {
        ReturnDefault<IEnumerable<RegistrosInterpoladosDto>> returndefault = new();

        var isfazenda = await _fazendaRepository.AnyAsync(w => w.Id == filter.FazendaId);

        if (!isfazenda) 
        {
            returndefault.Message = "O Código da fazenda informado não foi encontrado";
            returndefault.StatusCode = HttpStatusCode.BadRequest;

            return returndefault;
        }
        
       var ismapafazendalavoura = await _fazendaRepository.LavouraExistsInFazendaAsync(filter.FazendaId, filter.CodigoLavoura);

        if (!ismapafazendalavoura)
        {
            returndefault.Message = "Lavoura não encontrada para a fazenda informada.";
            returndefault.StatusCode = HttpStatusCode.BadRequest;

            return returndefault;
        }

        var cacheKey = BuildCacheKey(filter);

        var cached = await _cache.GetStringAsync(cacheKey);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit: {Key}", cacheKey);
            returndefault.Data = JsonSerializer.Deserialize<IEnumerable<RegistrosInterpoladosDto>>(cached)!;

            return returndefault;
        }

        var result = await _repository.GetByFilterAsync(filter);
        returndefault.Data = result;
        
        await _cache.SetStringAsync(cacheKey,
            JsonSerializer.Serialize(result),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtl });

        return returndefault;
    }

    private static string BuildCacheKey(RegistrosInterpoladosFilterDto f)
        => $"ri:f{f.FazendaId}:l{f.CodigoLavoura ?? "all"}:d{f.DataHora:yyyyMMddHH}";
}