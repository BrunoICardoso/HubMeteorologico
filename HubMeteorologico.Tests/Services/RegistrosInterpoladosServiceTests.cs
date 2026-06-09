using System.Net;
using System.Text.Json;
using HubMeteorologico.Domain.DTOs.RegistrosInterpolados;
using HubMeteorologico.Domain.Interfaces.Services;
using HubMeteorologico.Domain.Services;
using HubMeteorologico.Infrastructure.Repository.Interface;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;

namespace HubMeteorologico.Tests.Services;

public class RegistrosInterpoladosServiceTests
{
    private readonly Mock<IRegistrosInterpoladosRepository> _repositoryMock;
    private readonly Mock<IFazendaRepository> _fazendaRepositoryMock;
    private readonly Mock<IMapaFazendaLavouraRepository> _mapaFazendaLavouraRepositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<ILogger<RegistrosInterpoladosService>> _loggerMock;
    private readonly RegistrosInterpoladosService _service;

    public RegistrosInterpoladosServiceTests()
    {
        _repositoryMock = new Mock<IRegistrosInterpoladosRepository>();
        _fazendaRepositoryMock = new Mock<IFazendaRepository>();
        _mapaFazendaLavouraRepositoryMock = new Mock<IMapaFazendaLavouraRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _loggerMock = new Mock<ILogger<RegistrosInterpoladosService>>();

        _service = new RegistrosInterpoladosService(
            _repositoryMock.Object,
            _fazendaRepositoryMock.Object,
            _mapaFazendaLavouraRepositoryMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetAsync_FazendaNaoEncontrada_RetornaBadRequest()
    {
        var filter = new RegistrosInterpoladosFilterDto
        {
            FazendaId = 99,
            CodigoLavoura = "LAV01",
            DataHora = DateTime.UtcNow
        };

        _fazendaRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Fazendas, bool>>>()))
            .ReturnsAsync(false);

        var result = await _service.GetAsync(filter);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("O Código da fazenda informado não foi encontrado", result.Message);
    }

    [Fact]
    public async Task GetAsync_LavouraNotFoundInFazenda_RetornaBadRequest()
    {
        var filter = new RegistrosInterpoladosFilterDto
        {
            FazendaId = 1,
            CodigoLavoura = "LAV_INVALIDA",
            DataHora = DateTime.UtcNow
        };

        _fazendaRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Fazendas, bool>>>()))
            .ReturnsAsync(true);

        _fazendaRepositoryMock
            .Setup(r => r.LavouraExistsInFazendaAsync(filter.FazendaId, filter.CodigoLavoura))
            .ReturnsAsync(false);

        var result = await _service.GetAsync(filter);

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.Equal("Lavoura não encontrada para a fazenda informada.", result.Message);
    }

    [Fact]
    public async Task GetAsync_CacheHit_RetornaDadosDoCache()
    {
        var filter = new RegistrosInterpoladosFilterDto
        {
            FazendaId = 1,
            CodigoLavoura = "LAV01",
            DataHora = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        var cachedData = new List<RegistrosInterpoladosDto>
        {
            new() { FazendaId = 1, DataHora = filter.DataHora, Temperatura = 25.5 }
        };

        _fazendaRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Fazendas, bool>>>()))
            .ReturnsAsync(true);

        _fazendaRepositoryMock
            .Setup(r => r.LavouraExistsInFazendaAsync(filter.FazendaId, filter.CodigoLavoura))
            .ReturnsAsync(true);

        var serialized = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cachedData));
        _cacheMock
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(serialized);

        var result = await _service.GetAsync(filter);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Data);
        _repositoryMock.Verify(r => r.GetByFilterAsync(It.IsAny<RegistrosInterpoladosFilterDto>()), Times.Never);
    }

    [Fact]
    public async Task GetAsync_CacheMiss_ConsultaRepositorioEArmazenaCache()
    {
        var filter = new RegistrosInterpoladosFilterDto
        {
            FazendaId = 1,
            CodigoLavoura = "LAV01",
            DataHora = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        var dbData = new List<RegistrosInterpoladosDto>
        {
            new() { FazendaId = 1, DataHora = filter.DataHora, Temperatura = 22.0, VolumeChuva = 5.0 }
        };

        _fazendaRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Fazendas, bool>>>()))
            .ReturnsAsync(true);

        _fazendaRepositoryMock
            .Setup(r => r.LavouraExistsInFazendaAsync(filter.FazendaId, filter.CodigoLavoura))
            .ReturnsAsync(true);

        _cacheMock
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _repositoryMock
            .Setup(r => r.GetByFilterAsync(filter))
            .ReturnsAsync(dbData);

        var result = await _service.GetAsync(filter);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(result.Data);
        _repositoryMock.Verify(r => r.GetByFilterAsync(filter), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_FiltroSemLavoura_UsaChaveCacheComAllLavoura()
    {
        var filter = new RegistrosInterpoladosFilterDto
        {
            FazendaId = 2,
            CodigoLavoura = null,
            DataHora = new DateTime(2024, 7, 15, 8, 0, 0, DateTimeKind.Utc)
        };

        _fazendaRepositoryMock
            .Setup(r => r.AnyAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Entities.Fazendas, bool>>>()))
            .ReturnsAsync(true);

        _fazendaRepositoryMock
            .Setup(r => r.LavouraExistsInFazendaAsync(filter.FazendaId, filter.CodigoLavoura!))
            .ReturnsAsync(true);

        _cacheMock
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _repositoryMock
            .Setup(r => r.GetByFilterAsync(filter))
            .ReturnsAsync(Enumerable.Empty<RegistrosInterpoladosDto>());

        var result = await _service.GetAsync(filter);

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        _cacheMock.Verify(c => c.SetAsync(
            It.Is<string>(k => k.Contains(":lall:")),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
