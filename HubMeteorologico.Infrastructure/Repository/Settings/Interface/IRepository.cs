using System.Linq.Expressions;

namespace HubMeteorologico.Infrastructure.Repository.Settings.Interface;

public interface IRepository<TEntity> where TEntity : class
{
    Task AddAsync(TEntity entity);
    Task<TIdentity> AddGetIdentityAsync<TIdentity>(TEntity entity);
    Task<TEntity> AddGetEntityAsync(TEntity entity);

    Task UpdateAsync(TEntity entity);
    Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> whereExpression);

    Task DeleteAsync(Expression<Func<TEntity, bool>> whereExpression);
    Task DeleteAsync(TEntity entity);

    Task<IEnumerable<TEntity>> GetAllAsync();
    Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> whereExpression);

    Task<IEnumerable<TModel>> GetAllAsync<TModel>(
        Expression<Func<TEntity, bool>> whereExpression,
        Expression<Func<TEntity, TModel>> selectExpression,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = true);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> whereExpression);

    Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> whereExpression);

    Task<TModel> GetByIdAsync<TModel>(
        Expression<Func<TEntity, bool>> whereExpression,
        Expression<Func<TEntity, TModel>>? selectExpression = null,
        Expression<Func<TEntity, object>>? orderBy = null,
        bool ascending = true);

    Task ExecuteProcedureAsync(string procedureName, object parameters = null, bool forWrite = true);

    Task<IEnumerable<TModel>> QueryAsync<TModel>(string sql, object? parameters = null);
    Task<TModel> QuerySingleAsync<TModel>(string sql, object? parameters = null);
}
