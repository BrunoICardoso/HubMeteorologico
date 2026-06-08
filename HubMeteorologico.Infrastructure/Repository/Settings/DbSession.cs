using HubMeteorologico.Infrastructure.Repository.Settings.Interface;
using System.Data;

namespace HubMeteorologico.Infrastructure.Repository.Settings;

public sealed class DbSession : IDbSession
{
    private readonly IDbConnection _connection;
    private IDbTransaction? _transaction;
    private bool _disposed;

    public DbSession(IDatabaseFactory databaseFactory)
    {
        _connection = databaseFactory.CreateConnection()
            ?? throw new InvalidOperationException("Conexão inválida.");

        if (_connection.State != ConnectionState.Open)
            _connection.Open();
    }

    public IDbConnection Connection => _connection;
    public IDbTransaction? Transaction => _transaction;

    public IDbTransaction EnsureTransaction()
        => _transaction ??= _connection.BeginTransaction(IsolationLevel.ReadCommitted);

    public void Commit()
    {
        if (_transaction == null)
            return;

        try
        {
            _transaction.Commit();
        }
        finally
        {
            ResetTransaction();
        }
    }

    public void Rollback()
    {
        if (_transaction == null)
            return;

        try
        {
            _transaction.Rollback();
        }
        finally
        {
            ResetTransaction();
        }
    }

    private void ResetTransaction()
    {
        _transaction?.Dispose();
        _transaction = null;
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            _transaction?.Dispose();
            _connection.Dispose();
        }
        finally
        {
            _disposed = true;
        }
    }
}
