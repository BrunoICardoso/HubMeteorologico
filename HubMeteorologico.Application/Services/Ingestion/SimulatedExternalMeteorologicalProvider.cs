using HubMeteorologico.Domain.DTOs.Ingestion;
using HubMeteorologico.Domain.Interfaces;

namespace HubMeteorologico.Domain.Services.Ingestion;

public class SimulatedExternalMeteorologicalProvider : IExternalMeteorologicalProvider
{
    public Task<IReadOnlyCollection<ExternalMeteorologicalReadingDto>> FetchReadingsAsync(
        IReadOnlyCollection<EquipamentoIngestionDto> equipamentos,
        DateTime dataHora,
        int anoAgricolaId,
        CancellationToken cancellationToken = default)
    {
        var readings = equipamentos
            .Select(e => BuildReading(e, dataHora, anoAgricolaId))
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<ExternalMeteorologicalReadingDto>>(readings);
    }

    private static ExternalMeteorologicalReadingDto BuildReading(
        EquipamentoIngestionDto equipamento,
        DateTime dataHora,
        int anoAgricolaId)
    {
        var seed = HashCode.Combine(equipamento.Id, dataHora);
        var random = new Random(seed);
        var temperatura = Math.Round(18 + random.NextDouble() * 14, 2);
        var volumeChuva = Math.Round(random.NextDouble() * 8, 2);
        var velocidadeVento = Math.Round(random.NextDouble() * 25, 2);

        return new ExternalMeteorologicalReadingDto
        {
            EquipamentoId = equipamento.Id,
            FazendaId = equipamento.FazendaId,
            DataHora = dataHora,
            AnoAgricolaId = anoAgricolaId,
            Consolidada = true,
            PressaoAtmosferica = Math.Round(990 + random.NextDouble() * 35, 2),
            UmidadeRelativaAr = Math.Round(45 + random.NextDouble() * 45, 2),
            VolumeChuva = volumeChuva,
            Temperatura = temperatura,
            TemperaturaMaxima = Math.Round(temperatura + random.NextDouble() * 3, 2),
            TemperaturaMinima = Math.Round(temperatura - random.NextDouble() * 3, 2),
            DirecaoVento = Math.Round(random.NextDouble() * 360, 2),
            VelocidadeVento = velocidadeVento,
            VelocidadeVentoPico = Math.Round(velocidadeVento + random.NextDouble() * 12, 2),
            PontoOrvalho = Math.Round(temperatura - random.NextDouble() * 6, 2),
            Bateria = Math.Round(70 + random.NextDouble() * 30, 2),
            FolhaMolhada = Math.Round(random.NextDouble() * 100, 2),
            Versao = "simulated-provider-v1",
            RadiacaoSolar = Math.Round(random.NextDouble() * 900, 2),
            Evapotranspiracao = Math.Round(random.NextDouble() * 6, 2)
        };
    }
}
