using HubMeteorologico.Infrastructure.Repository.Settings.Interface;
using Npgsql;
using System.Data;

namespace HubMeteorologico.Infrastructure.Repository.Settings;

public class DatabaseFactory : IDatabaseFactory
{
    private readonly NpgsqlDataSource _dataSource;

    public DatabaseFactory(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public IDbConnection CreateConnection()
    {
        return _dataSource.CreateConnection();
    }
}