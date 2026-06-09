using Dapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using HubMeteorologico.Infrastructure.Repository.Settings.Interface;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace HubMeteorologico.Infrastructure.Repository.Settings;

/// <summary>
/// Repositório genérico com CRUD via reflection (Dapper) + transação lazy (IDbSession).
/// </summary>
public class Repository<TEntity> : RepositoryBase, IRepository<TEntity>
    where TEntity : class
{
    public Repository(IDbSession session) : base(session) { }

    #region Public API

    public async Task AddAsync(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var insert = BuildInsertClause(entity, includeIdentity: false);

        var sql = $@"INSERT INTO {GetTableName()} ({insert.Columns})
                         VALUES ({insert.Values});";

        await Conn.ExecuteAsync(sql, insert.Parameters, transaction: Tx(true));
    }

    public async Task<TIdentity> AddGetIdentityAsync<TIdentity>(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var keyProp = GetKeyProperty();
        var keyColumn = GetColumnName(keyProp);

        var insert = BuildInsertClause(entity, includeIdentity: false);

        var sql = $@"INSERT INTO {GetTableName()} ({insert.Columns})
                         VALUES ({insert.Values})
                         RETURNING {keyColumn};";

        return await Conn.QuerySingleAsync<TIdentity>(sql, insert.Parameters, transaction: Tx(true));
    }

    public async Task<TEntity> AddGetEntityAsync(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var insert = BuildInsertClause(entity, includeIdentity: false);

        var sql = $@"INSERT INTO {GetTableName()} ({insert.Columns})
                         VALUES ({insert.Values})
                         RETURNING *;";

        return await Conn.QuerySingleAsync<TEntity>(sql, insert.Parameters, transaction: Tx(true));
    }

    public async Task UpdateAsync(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var keyProp = GetKeyProperty();
        var keyColumn = GetColumnName(keyProp);
        var keyValue = keyProp.GetValue(entity);

        var update = BuildUpdateClause(entity);

        var dp = new DynamicParameters(update.Parameters);
        dp.Add($"__key", keyValue);

        var sql = $@"UPDATE {GetTableName()}
                         SET {update.SetClause}
                         WHERE {keyColumn} = @__key;";

        await Conn.ExecuteAsync(sql, dp, transaction: Tx(true));
    }

    public async Task UpdateAsync(TEntity entity, Expression<Func<TEntity, bool>> whereExpression)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));

        var update = BuildUpdateClause(entity);
        var where = BuildWhereClause(whereExpression);

        var dp = new DynamicParameters(update.Parameters);
        foreach (var name in where.Parameters.ParameterNames)
            dp.Add(name, where.Parameters.Get<dynamic>(name));

        var sql = $@"UPDATE {GetTableName()}
                         SET {update.SetClause}
                         WHERE {where.WhereSql};";

        await Conn.ExecuteAsync(sql, dp, transaction: Tx(true));
    }

    public async Task DeleteAsync(Expression<Func<TEntity, bool>> whereExpression)
    {
        if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));

        var where = BuildWhereClause(whereExpression);

        var sql = $@"DELETE FROM {GetTableName()}
                         WHERE {where.WhereSql};";

        await Conn.ExecuteAsync(sql, where.Parameters, transaction: Tx(true));
    }

    public async Task DeleteAsync(TEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        var keyProp = GetKeyProperty();
        var keyColumn = GetColumnName(keyProp);
        var keyValue = keyProp.GetValue(entity);

        var sql = $@"DELETE FROM {GetTableName()}
                         WHERE {keyColumn} = @__key;";

        await Conn.ExecuteAsync(sql, new { __key = keyValue }, transaction: Tx(true));
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        var sql = $@"SELECT {BuildSelectClause()}
                         FROM {GetTableName()};";

        return await Conn.QueryAsync<TEntity>(sql, transaction: Tx(false));
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> whereExpression)
    {
        if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));

        var where = BuildWhereClause(whereExpression);

        var sql = $@"SELECT {BuildSelectClause()}
                         FROM {GetTableName()}
                         WHERE {where.WhereSql};";

        return await Conn.QueryAsync<TEntity>(sql, where.Parameters, transaction: Tx(false));
    }

    public async Task<IEnumerable<TModel>> GetAllAsync<TModel>(
        Expression<Func<TEntity, bool>> whereExpression,
        Expression<Func<TEntity, TModel>> selectExpression,
        Expression<Func<TEntity, object>> orderBy = null,
        bool ascending = true)
    {
        if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));
        if (selectExpression == null) throw new ArgumentNullException(nameof(selectExpression));

        var where = BuildWhereClause(whereExpression);
        var selectedColumns = BuildSelectClause(selectExpression);
        var orderByClause = BuildOrderByClause(orderBy, ascending);

        var sql = $@"SELECT {selectedColumns}
                         FROM {GetTableName()}
                         WHERE {where.WhereSql}
                         {orderByClause};";

        return await Conn.QueryAsync<TModel>(sql, where.Parameters, transaction: Tx(false));
    }

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> whereExpression)
    {
        if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));

        var where = BuildWhereClause(whereExpression);

        var sql = $@"SELECT 1
                         FROM {GetTableName()}
                         WHERE {where.WhereSql}
                         LIMIT 1;";

        var result = await Conn.QueryFirstOrDefaultAsync<int?>(sql, where.Parameters, transaction: Tx(false));
        return result.HasValue;
    }

    public async Task<TEntity> GetByIdAsync(Expression<Func<TEntity, bool>> whereExpression)
    {
        if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));

        var where = BuildWhereClause(whereExpression);

        var sql = $@"SELECT {BuildSelectClause()}
                         FROM {GetTableName()}
                         WHERE {where.WhereSql}
                         LIMIT 1;";

        return await Conn.QueryFirstOrDefaultAsync<TEntity>(sql, where.Parameters, transaction: Tx(false));
    }

    public async Task<TModel> GetByIdAsync<TModel>(
        Expression<Func<TEntity, bool>> whereExpression,
        Expression<Func<TEntity, TModel>> selectExpression = null,
        Expression<Func<TEntity, object>> orderBy = null,
        bool ascending = true)
    {
        if (whereExpression == null) throw new ArgumentNullException(nameof(whereExpression));

        var where = BuildWhereClause(whereExpression);
        var selectedColumns = selectExpression != null ? BuildSelectClause(selectExpression) : BuildSelectClause();
        var orderByClause = BuildOrderByClause(orderBy, ascending);

        var sql = $@"SELECT {selectedColumns}
                         FROM {GetTableName()}
                         WHERE {where.WhereSql}
                         {orderByClause}
                         LIMIT 1;";

        return await Conn.QueryFirstOrDefaultAsync<TModel>(sql, where.Parameters, transaction: Tx(false));
    }

    public async Task ExecuteProcedureAsync(string procedureName, object parameters = null, bool forWrite = true)
    {
        if (string.IsNullOrWhiteSpace(procedureName))
            throw new ArgumentException("Procedure name is required.", nameof(procedureName));

        await Conn.ExecuteAsync(
            procedureName,
            parameters,
            commandType: CommandType.StoredProcedure,
            transaction: Tx(forWrite));
    }

    public async Task<IEnumerable<TModel>> QueryAsync<TModel>(string sql, object parameters = null)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL is required.", nameof(sql));

        return await Conn.QueryAsync<TModel>(sql, parameters, transaction: Tx(false));
    }

    public async Task<TModel> QuerySingleAsync<TModel>(string sql, object parameters = null)
    {
        if (string.IsNullOrWhiteSpace(sql))
            throw new ArgumentException("SQL is required.", nameof(sql));

        return await Conn.QueryFirstOrDefaultAsync<TModel>(sql, parameters, transaction: Tx(false));
    }

    #endregion

    #region Reflection mapping helpers

    private string GetTableName()
    {
        var tableAttribute = typeof(TEntity).GetCustomAttribute<TableAttribute>();
        return $"\"{tableAttribute?.Schema ?? "public"}\".\"{tableAttribute?.Name ?? typeof(TEntity).Name}\"";
    }

    private static PropertyInfo GetKeyProperty()
    {
        var type = typeof(TEntity);

        var keyProp = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);

        if (keyProp != null)
            return keyProp;

        // fallback: convenção "Id" ou "{Entity}Id"
        keyProp = type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)
                  ?? type.GetProperty($"{type.Name}Id", BindingFlags.Public | BindingFlags.Instance);

        if (keyProp == null)
            throw new InvalidOperationException($"Não foi encontrada PK ([Key]) em {type.Name}.");

        return keyProp;
    }

    private static bool IsNotMapped(PropertyInfo p)
        => p.GetCustomAttribute<NotMappedAttribute>() != null;

    private static string GetColumnName(PropertyInfo p)
    {
        var col = p.GetCustomAttribute<ColumnAttribute>();
        if (col != null && !string.IsNullOrWhiteSpace(col.Name))
            return col.Name;

        return p.Name;
    }

    private static bool IsIdentity(PropertyInfo p)
    {
        var dbGen = p.GetCustomAttribute<DatabaseGeneratedAttribute>();
        return dbGen != null && dbGen.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity;
    }

    private static IEnumerable<PropertyInfo> GetMappedProperties(bool includeKey, bool includeIdentity)
    {
        var type = typeof(TEntity);
        var key = GetKeyProperty();

        foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!p.CanRead) continue;
            if (IsNotMapped(p)) continue;

            var isKey = p == key;
            if (isKey && !includeKey) continue;

            if (IsIdentity(p) && !includeIdentity)
                continue;

            yield return p;
        }
    }

    private sealed class InsertClause
    {
        public string Columns { get; init; }
        public string Values { get; init; }
        public DynamicParameters Parameters { get; init; }
    }

    private static InsertClause BuildInsertClause(TEntity entity, bool includeIdentity)
    {
        // regra: não inclui PK/Identity por padrão (Postgres geralmente usa serial/identity)
        var key = GetKeyProperty();

        var props = GetMappedProperties(includeKey: false, includeIdentity: includeIdentity).ToList();
        if (props.Count == 0)
            throw new InvalidOperationException("Nenhuma coluna mapeada encontrada para INSERT.");

        var cols = new List<string>(props.Count);
        var vals = new List<string>(props.Count);
        var dp = new DynamicParameters();

        foreach (var p in props)
        {
            var col = GetColumnName(p);
            cols.Add(col);

            // nome do parâmetro usa o nome da propriedade pra facilitar binding
            var paramName = p.Name;
            vals.Add("@" + paramName);

            AddParameter(dp, p, paramName, p.GetValue(entity));
        }

        return new InsertClause
        {
            Columns = string.Join(", ", cols.Select(p => $"\"{p}\"")),
            Values = string.Join(", ", vals),
            Parameters = dp
        };
    }

    private sealed class UpdateClause
    {
        public string SetClause { get; init; }
        public DynamicParameters Parameters { get; init; }
    }

    private static UpdateClause BuildUpdateClause(TEntity entity)
    {
        var key = GetKeyProperty();

        var props = GetMappedProperties(includeKey: false, includeIdentity: true).ToList();
        if (props.Count == 0)
            throw new InvalidOperationException("Nenhuma coluna mapeada encontrada para UPDATE.");

        var setParts = new List<string>(props.Count);
        var dp = new DynamicParameters();

        foreach (var p in props)
        {
            // não atualiza PK
            if (p == key) continue;
            if (!p.CanRead) continue;

            var col = GetColumnName(p);
            var paramName = p.Name;
            
            setParts.Add($"\"{col}\" = @{paramName}");
            
            AddParameter(dp, p, paramName, p.GetValue(entity));
        }

        return new UpdateClause
        {
            SetClause = string.Join(", ", setParts),
            Parameters = dp
        };
    }

    private static string BuildSelectClause()
    {
        // Seleciona todas as colunas mapeadas (inclui PK)
        var props = GetMappedProperties(includeKey: true, includeIdentity: true).ToList();
        if (props.Count == 0) return "*";

        return string.Join(", ", (props.Select(GetColumnName)).Select(p => $"\"{p}\""));
    }

    private static string BuildSelectClause<TModel>(Expression<Func<TEntity, TModel>> selectExpression)
    {
        var cols = BuildSelectedColumns(selectExpression.Body);
        if (cols.Count == 0)
        {
            return string.Join(", ", BuildSelectClause().Select(p => $"\"{p}\""));
        }

        return string.Join(", ", cols.Select(p=> $"\"{p}\""));
    }

    private static List<string> BuildSelectedColumns(Expression body)
    {
        // suporta:
        // x => x.Prop
        // x => new { x.A, x.B }
        // x => new Model { A = x.A, B = x.B }
        var cols = new List<string>();

        if (body is UnaryExpression u && u.NodeType == ExpressionType.Convert)
            body = u.Operand;

        if (body is MemberExpression me)
        {
            cols.Add(GetColumnName((PropertyInfo)me.Member));
            return cols;
        }

        if (body is NewExpression ne)
        {
            foreach (var arg in ne.Arguments)
            {
                var m = StripConvert(arg) as MemberExpression;
                if (m?.Member is PropertyInfo pi)
                    cols.Add(GetColumnName(pi));
            }
            return cols;
        }

        if (body is MemberInitExpression mi)
        {
            foreach (var b in mi.Bindings.OfType<MemberAssignment>())
            {
                var m = StripConvert(b.Expression) as MemberExpression;
                if (m?.Member is PropertyInfo pi)
                    cols.Add(GetColumnName(pi));
            }
            return cols;
        }

        return cols;
    }

    private static Expression StripConvert(Expression e)
    {
        while (e is UnaryExpression u && u.NodeType == ExpressionType.Convert)
            e = u.Operand;

        return e;
    }

    private static string BuildOrderByClause(Expression<Func<TEntity, object>> orderBy, bool ascending)
    {
        if (orderBy == null) return string.Empty;

        Expression body = orderBy.Body;
        body = StripConvert(body);

        if (body is MemberExpression me && me.Member is PropertyInfo pi)
        {
            var col = GetColumnName(pi);
            return $"ORDER BY {col} {(ascending ? "ASC" : "DESC")}";
        }

        return string.Empty;
    }

    #endregion

    #region Expression -> SQL WHERE

    private sealed class WhereClauseResult
    {
        public string WhereSql { get; init; }
        public DynamicParameters Parameters { get; init; }
    }

    private static WhereClauseResult BuildWhereClause(Expression<Func<TEntity, bool>> whereExpression)
    {
        var dp = new DynamicParameters();
        int paramIndex = 0;

        string Visit(Expression exp)
        {
            exp = StripConvert(exp);

            switch (exp.NodeType)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    {
                        var be = (BinaryExpression)exp;
                        var left = Visit(be.Left);
                        var right = Visit(be.Right);
                        var op = exp.NodeType == ExpressionType.AndAlso ? "AND" : "OR";
                        return $"(\"{left}\" {op} {right})";
                    }

                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    {
                        var be = (BinaryExpression)exp;
                        var (col, val) = ExtractColumnAndValue(be.Left, be.Right);

                        var pName = $"p{paramIndex++}";
                        dp.Add(pName, val);

                        var op = exp.NodeType switch
                        {
                            ExpressionType.Equal => "=",
                            ExpressionType.NotEqual => "<>",
                            ExpressionType.GreaterThan => ">",
                            ExpressionType.GreaterThanOrEqual => ">=",
                            ExpressionType.LessThan => "<",
                            ExpressionType.LessThanOrEqual => "<=",
                            _ => throw new NotSupportedException()
                        };

                        // null handling: = null não funciona em SQL
                        if (val == null)
                        {
                            return exp.NodeType == ExpressionType.Equal
                                ? $"\"{col}\" IS NULL"
                                : $"\"{col}\" IS NOT NULL";                        }

                        return $"\"{col}\" {op} @{pName}";
                    }

                default:
                    throw new NotSupportedException($"Expressão WHERE não suportada: {exp.NodeType} ({exp}).");
            }
        }

        var whereSql = Visit(whereExpression.Body);

        return new WhereClauseResult
        {
            WhereSql = whereSql,
            Parameters = dp
        };
    }

    private static (string columnSql, object value) ExtractColumnAndValue(Expression left, Expression right)
    {
        // tenta reconhecer "x.Prop OP const"
        // e também "const OP x.Prop"
        left = StripConvert(left);
        right = StripConvert(right);

        if (left is MemberExpression lm && lm.Member is PropertyInfo lpi && IsEntityParameter(lm))
        {
            var col = GetColumnName(lpi);
            var val = Evaluate(right);
            return (col, val);
        }

        if (right is MemberExpression rm && rm.Member is PropertyInfo rpi && IsEntityParameter(rm))
        {
            var col = GetColumnName(rpi);
            var val = Evaluate(left);
            return (col, val);
        }

        throw new NotSupportedException($"Não foi possível extrair coluna/valor do WHERE: {left} / {right}");
    }

    private static bool IsEntityParameter(MemberExpression me)
    {
        // confere se é algo tipo x.Prop (x é o parâmetro da lambda)
        Expression? current = me.Expression;
        while (current is MemberExpression inner)
            current = inner.Expression;

        return current is ParameterExpression;
    }

    private static object Evaluate(Expression expr)
    {
        expr = StripConvert(expr);

        if (expr is ConstantExpression c)
            return c.Value;

        // Captured variables / MemberAccess -> compilar e executar
        var lambda = Expression.Lambda(expr);
        var compiled = lambda.Compile();
        return compiled.DynamicInvoke();
    }

    #endregion


   private static void AddParameter(DynamicParameters parameters,PropertyInfo property,string parameterName,object? value)
    {
        if (value is Geometry geometry)
        {
            var column = property.GetCustomAttribute<ColumnAttribute>();

            var isGeography = column?.TypeName?.Contains(
                "geography",
                StringComparison.OrdinalIgnoreCase) == true;

            if (isGeography)
            {
                parameters.Add(parameterName, new GeographyParameter(geometry));
                return;
            }

            parameters.Add(parameterName, new GeometryParameter(geometry));
            return;
        }

        parameters.Add(parameterName, value);
    }

}
