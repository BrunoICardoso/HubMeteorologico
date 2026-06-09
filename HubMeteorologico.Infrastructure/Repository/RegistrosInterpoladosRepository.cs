using Dapper;
using HubMeteorologico.Domain.DTOs.RegistrosInterpolados;
using HubMeteorologico.Domain.Entities;
using HubMeteorologico.Infrastructure.Repository.Interface;
using HubMeteorologico.Infrastructure.Repository.Settings;

namespace HubMeteorologico.Infrastructure.Repository;

public class RegistrosInterpoladosRepository : Repository<RegistrosInterpolados>, IRegistrosInterpoladosRepository
{
    public RegistrosInterpoladosRepository(IDbSession session) : base(session) { }

    public async Task<IEnumerable<RegistrosInterpoladosDto>> GetByFilterAsync(RegistrosInterpoladosFilterDto filter)
    {
        // DataHora is stored as "hora cheia" — match exact timestamp
        // CodigoLavoura is optional; when supplied we join MapaFazendaLavoura to filter
        const string baseSql = @"
            SELECT
                ri.""DataHora"",
                ri.""FazendaId"",
                ri.""MapaFazendaId"",
                ri.""MapaFazendaLavouraId"",
                ri.""MapaFazendaLavouraInterpolacaoId"",
                ri.""AnoAgricolaId"",
                ST_Y(ri.""Coordenada""::geometry)  AS ""Latitude"",
                ST_X(ri.""Coordenada""::geometry)  AS ""Longitude"",
                ri.""Consolidada"",
                ri.""VolumeChuva"",
                ri.""PressaoAtmosferica"",
                ri.""UmidadeRelativaAr"",
                ri.""Temperatura"",
                ri.""TemperaturaMaxima"",
                ri.""TemperaturaMinima"",
                ri.""DirecaoVento"",
                ri.""VelocidadeVento"",
                ri.""VelocidadeVentoPico"",
                ri.""PontoOrvalho"",
                ri.""FolhaMolhada"",
                ri.""RadiacaoSolar"",
                ri.""Evapotranspiracao""
            FROM public.""RegistrosInterpolados"" ri
            INNER JOIN public.""MapaFazendaLavoura"" mfl
                ON mfl.""Id"" = ri.""MapaFazendaLavouraId""
            WHERE
                ri.""FazendaId""  = @FazendaId
                AND ri.""DataHora"" = @DataHora
                {0}
            ORDER BY ri.""MapaFazendaLavouraInterpolacaoId"";";

        var lavouraClause = string.IsNullOrWhiteSpace(filter.CodigoLavoura)
            ? string.Empty
            : @"AND mfl.""CodigoLavoura"" = @CodigoLavoura";

        var sql = string.Format(baseSql, lavouraClause);

        return await Conn.QueryAsync<RegistrosInterpoladosDto>(sql, new
        {
            filter.FazendaId,
            filter.DataHora,
            filter.CodigoLavoura
        }, transaction: Tx(false));
    }
}