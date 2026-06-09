using HubMeteorologico.Domain.DTOs.Ingestion;
using HubMeteorologico.Domain.Interfaces;
using HubMeteorologico.Domain.Services.Ingestion;
using HubMeteorologico.Infrastructure.Repository.Interface;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace HubMeteorologico.Tests.Services;

public class MeteorologicalIngestionServiceTests
{
    [Fact]
    public async Task ImportFullHourAsync_QuandoNaoExisteAnoAgricola_NaoBuscaEquipamentosNemEnfileira()
    {
        var repository = new Mock<IRegistrosMeteorologicosRepository>();
        var provider = new Mock<IExternalMeteorologicalProvider>();
        var queue = new Mock<IInterpolationQueue>();
        var service = new MeteorologicalIngestionService(
            repository.Object,
            provider.Object,
            queue.Object,
            Mock.Of<ILogger<MeteorologicalIngestionService>>());

        repository
            .Setup(r => r.GetAnoAgricolaIdAsync(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int?)null);

        var result = await service.ImportFullHourAsync(new DateTime(2024, 6, 1, 10, 45, 0, DateTimeKind.Utc));

        Assert.Empty(result);
        repository.Verify(r => r.GetActiveEquipamentosAsync(It.IsAny<CancellationToken>()), Times.Never);
        provider.Verify(
            p => p.FetchReadingsAsync(
                It.IsAny<IReadOnlyCollection<EquipamentoIngestionDto>>(),
                It.IsAny<DateTime>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        queue.Verify(q => q.EnqueueAsync(It.IsAny<InterpolationRequestedMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ImportFullHourAsync_PersisteLeiturasEEnfileiraUmaMensagemPorFazendaHora()
    {
        var repository = new Mock<IRegistrosMeteorologicosRepository>();
        var provider = new Mock<IExternalMeteorologicalProvider>();
        var queue = new Mock<IInterpolationQueue>();
        var service = new MeteorologicalIngestionService(
            repository.Object,
            provider.Object,
            queue.Object,
            Mock.Of<ILogger<MeteorologicalIngestionService>>());

        var inputDate = new DateTime(2024, 6, 1, 10, 45, 0, DateTimeKind.Utc);
        var fullHour = new DateTime(2024, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var equipamentos = new[]
        {
            new EquipamentoIngestionDto { Id = 1, FazendaId = 10, Codigo = "EST-01" },
            new EquipamentoIngestionDto { Id = 2, FazendaId = 10, Codigo = "PLU-01" }
        };
        var readings = equipamentos
            .Select(e => new ExternalMeteorologicalReadingDto
            {
                EquipamentoId = e.Id,
                FazendaId = e.FazendaId,
                DataHora = fullHour,
                AnoAgricolaId = 5,
                VolumeChuva = 1
            })
            .ToArray();

        repository
            .Setup(r => r.GetAnoAgricolaIdAsync(fullHour, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        repository
            .Setup(r => r.GetActiveEquipamentosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipamentos);
        provider
            .Setup(p => p.FetchReadingsAsync(equipamentos, fullHour, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readings);
        repository
            .Setup(r => r.UpsertBatchAsync(readings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(readings.Length);

        var result = await service.ImportFullHourAsync(inputDate);

        var message = Assert.Single(result);
        Assert.Equal(10, message.FazendaId);
        Assert.Equal(fullHour, message.DataHora);
        Assert.Equal(2, message.ImportedReadings);
        queue.Verify(q => q.EnqueueAsync(It.Is<InterpolationRequestedMessage>(m =>
            m.FazendaId == 10 &&
            m.DataHora == fullHour &&
            m.ImportedReadings == 2), It.IsAny<CancellationToken>()), Times.Once);
    }
}
