using System.Data;

namespace HubMeteorologico.Infrastructure.Repository.Settings;

public interface IDbSession : IDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }
    
    // Cria transação apenas quando realmente for gravar
    IDbTransaction EnsureTransaction();
    void Commit();
    void Rollback();
}
