using HubMeteorologico.Infrastructure.Repository.Settings.Interface;

namespace HubMeteorologico.Infrastructure.Repository.Settings;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbSession _session;

    public UnitOfWork(IDbSession session)
    {
        _session = session;
    }

    public Task CommitAsync()
    {
        _session.Commit();
        return Task.CompletedTask;
    }

    public void Rollback()
    {
        _session.Rollback();
    }
}
