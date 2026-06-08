using System.Data;

namespace HubMeteorologico.Infrastructure.Repository.Settings.Interface;

public interface IDatabaseFactory
{
    IDbConnection CreateConnection();
}
