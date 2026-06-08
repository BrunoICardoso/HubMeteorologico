using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using Dapper;
using NetTopologySuite.Geometries;
using Npgsql;
using NpgsqlTypes;

public sealed class GeographyParameter : SqlMapper.ICustomQueryParameter
{
    private readonly Geometry _value;

    public GeographyParameter(Geometry value)
    {
        _value = value;
    }

    public void AddParameter(IDbCommand command, string name)
    {
        var parameter = new NpgsqlParameter(name, NpgsqlDbType.Geography)
        {
            Value = _value
        };

        command.Parameters.Add(parameter);
    }
}

public sealed class GeometryParameter : SqlMapper.ICustomQueryParameter
{
    private readonly Geometry _value;

    public GeometryParameter(Geometry value)
    {
        _value = value;
    }

    public void AddParameter(IDbCommand command, string name)
    {
        var parameter = new NpgsqlParameter(name, NpgsqlDbType.Geometry)
        {
            Value = _value
        };

        command.Parameters.Add(parameter);
    }
}
