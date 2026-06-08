namespace HubMeteorologico.Infrastructure.Repository.Settings.Interface;

public interface IUnitOfWork
{
    Task CommitAsync();
    void Rollback();
}
