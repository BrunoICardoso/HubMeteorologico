using System.Data;

namespace HubMeteorologico.Infrastructure.Repository.Settings;

/// <summary>
/// Base para repositórios com acesso ao IDbSession (Connection + Transaction lazy).
/// </summary>
public abstract class RepositoryBase
{
    protected readonly IDbSession Session;

    protected RepositoryBase(IDbSession session)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
    }

    protected IDbConnection Conn => Session.Connection;

    /// <summary>
    /// forWrite=true  => garante transação (lazy: cria no 1º write)
    /// forWrite=false => usa transação existente se houver (não cria)
    /// </summary>
    protected IDbTransaction? Tx(bool forWrite)
        => forWrite ? Session.EnsureTransaction() : Session.Transaction;
}


