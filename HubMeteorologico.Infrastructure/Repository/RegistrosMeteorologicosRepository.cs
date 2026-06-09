using Dapper;
using HubMeteorologico.Domain.DTOs.Ingestion;
using HubMeteorologico.Domain.Entities;
using HubMeteorologico.Infrastructure.Repository.Interface;
using HubMeteorologico.Infrastructure.Repository.Settings;

namespace HubMeteorologico.Infrastructure.Repository;

public class RegistrosMeteorologicosRepository : Repository<RegistrosMeteorologicos>, IRegistrosMeteorologicosRepository
{
    public RegistrosMeteorologicosRepository(IDbSession session) : base(session) { }

    public async Task<IReadOnlyCollection<EquipamentoIngestionDto>> GetActiveEquipamentosAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT
                e.""Id"",
                e.""FazendaId"",
                e.""TipoEquipamento"",
                e.""FonteMeteorologica"",
                e.""Codigo""
            FROM public.""Equipamentos"" e
            WHERE e.""Ativo"" = TRUE
            ORDER BY e.""FazendaId"", e.""Id"";";

        var result = await Conn.QueryAsync<EquipamentoIngestionDto>(new CommandDefinition(
            sql,
            transaction: Tx(false),
            cancellationToken: cancellationToken));

        return result.ToArray();
    }

    public async Task<int?> GetAnoAgricolaIdAsync(DateTime dataHora, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT a.""Id""
            FROM public.""AnosAgricolas"" a
            WHERE @DataHora >= a.""DataInicial""
              AND @DataHora <= a.""DataFinal""
            ORDER BY a.""DataInicial"" DESC
            LIMIT 1;";

        return await Conn.QueryFirstOrDefaultAsync<int?>(new CommandDefinition(
            sql,
            new { DataHora = dataHora },
            transaction: Tx(false),
            cancellationToken: cancellationToken));
    }

    public async Task<int> UpsertBatchAsync(
        IReadOnlyCollection<ExternalMeteorologicalReadingDto> readings,
        CancellationToken cancellationToken = default)
    {
        if (readings.Count == 0)
            return 0;

        const string sql = @"
            INSERT INTO public.""RegistrosMeteorologicos"" (
                ""DataHora"",
                ""FazendaId"",
                ""EquipamentoId"",
                ""Consolidada"",
                ""PressaoAtmosferica"",
                ""UmidadeRelativaAr"",
                ""VolumeChuva"",
                ""Temperatura"",
                ""DirecaoVento"",
                ""VelocidadeVento"",
                ""PontoOrvalho"",
                ""Bateria"",
                ""FolhaMolhada"",
                ""Versao"",
                ""RadiacaoSolar"",
                ""CreatorUsername"",
                ""CreationTime"",
                ""ModificationTime"",
                ""ModifierUsername"",
                ""AnoAgricolaId"",
                ""TemperaturaMaxima"",
                ""TemperaturaMinima"",
                ""VelocidadeVentoPico"",
                ""Evapotranspiracao""
            )
            VALUES (
                @DataHora,
                @FazendaId,
                @EquipamentoId,
                @Consolidada,
                @PressaoAtmosferica,
                @UmidadeRelativaAr,
                @VolumeChuva,
                @Temperatura,
                @DirecaoVento,
                @VelocidadeVento,
                @PontoOrvalho,
                @Bateria,
                @FolhaMolhada,
                @Versao,
                @RadiacaoSolar,
                'worker-ingestao',
                NOW(),
                NULL,
                NULL,
                @AnoAgricolaId,
                @TemperaturaMaxima,
                @TemperaturaMinima,
                @VelocidadeVentoPico,
                @Evapotranspiracao
            )
            ON CONFLICT (""DataHora"", ""FazendaId"", ""EquipamentoId"")
            DO UPDATE SET
                ""Consolidada"" = EXCLUDED.""Consolidada"",
                ""PressaoAtmosferica"" = EXCLUDED.""PressaoAtmosferica"",
                ""UmidadeRelativaAr"" = EXCLUDED.""UmidadeRelativaAr"",
                ""VolumeChuva"" = EXCLUDED.""VolumeChuva"",
                ""Temperatura"" = EXCLUDED.""Temperatura"",
                ""DirecaoVento"" = EXCLUDED.""DirecaoVento"",
                ""VelocidadeVento"" = EXCLUDED.""VelocidadeVento"",
                ""PontoOrvalho"" = EXCLUDED.""PontoOrvalho"",
                ""Bateria"" = EXCLUDED.""Bateria"",
                ""FolhaMolhada"" = EXCLUDED.""FolhaMolhada"",
                ""Versao"" = EXCLUDED.""Versao"",
                ""RadiacaoSolar"" = EXCLUDED.""RadiacaoSolar"",
                ""ModificationTime"" = NOW(),
                ""ModifierUsername"" = 'worker-ingestao',
                ""AnoAgricolaId"" = EXCLUDED.""AnoAgricolaId"",
                ""TemperaturaMaxima"" = EXCLUDED.""TemperaturaMaxima"",
                ""TemperaturaMinima"" = EXCLUDED.""TemperaturaMinima"",
                ""VelocidadeVentoPico"" = EXCLUDED.""VelocidadeVentoPico"",
                ""Evapotranspiracao"" = EXCLUDED.""Evapotranspiracao"";";

        return await Conn.ExecuteAsync(new CommandDefinition(
            sql,
            readings,
            transaction: Tx(false),
            cancellationToken: cancellationToken));
    }
}
