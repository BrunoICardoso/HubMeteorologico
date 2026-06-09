using Dapper;
using HubMeteorologico.Domain.Entities;
using HubMeteorologico.Domain.Interfaces.Services;
using HubMeteorologico.Infrastructure.Repository.Settings;

namespace HubMeteorologico.Infrastructure.Repository;

public class FazendaRepository : Repository<Fazendas>, IFazendaRepository
{
    public FazendaRepository(IDbSession session) : base(session) { }

    public async Task<bool> LavouraExistsInFazendaAsync(int fazendaId, string codigoLavoura)
    {
        const string sql = @"
            SELECT EXISTS (
                SELECT 1
                FROM public.""MapaFazendaLavoura"" mfl
                INNER JOIN public.""MapaFazenda"" mf ON mf.""Id"" = mfl.""MapaFazendaId""
                WHERE mf.""FazendaId"" = @FazendaId
                  AND mfl.""CodigoLavoura"" = @CodigoLavoura
            );";
        return await Conn.ExecuteScalarAsync<bool>(sql, new { FazendaId = fazendaId, CodigoLavoura = codigoLavoura }, transaction: Tx(false));
    }
}