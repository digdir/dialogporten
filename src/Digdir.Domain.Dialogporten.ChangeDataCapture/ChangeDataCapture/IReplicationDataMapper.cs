using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;

public interface IReplicationDataMapper<T>
{
    Task<T> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct);

    Task<T> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct);
}

internal sealed class OutboxReplicationDataMapper : IReplicationDataMapper<OutboxMessage>
{
    public Task<OutboxMessage> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct)
    {
        var outboxMessageDictionary = reader.ToDictionary();
        return Task.FromResult(ToOutboxMessage(outboxMessageDictionary));
    }

    public async Task<OutboxMessage> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct)
    {
        var outboxMessageDictionary = await insertMessage.ToDictionary(ct);
        return ToOutboxMessage(outboxMessageDictionary);
    }

    private static OutboxMessage ToOutboxMessage(IReadOnlyDictionary<string, string> dic)
    {
        return new OutboxMessage
        {
            EventId = Guid.Parse(dic[nameof(OutboxMessage.EventId)]),
            EventType = dic[nameof(OutboxMessage.EventType)],
            EventPayload = dic[nameof(OutboxMessage.EventPayload)]
        };
    }
}

internal static class NpgsqlExtensions
{
    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropInfoConverter>> _propInfoConvertersByType = new();

    public static async Task<T> To<T>(this InsertMessage insertMessage, CancellationToken cancellationToken)
        where T : class
    {
        var result = Activator.CreateInstance<T>();
        var infoConverterByPropName = _propInfoConvertersByType.GetOrAdd(typeof(T), GeneratePropInfoConvertionsByPropName);
        var columnNumber = 0;
        await foreach (var value in insertMessage.NewRow)
        {
            var propName = insertMessage.Relation.Columns[columnNumber++].ColumnName;
            var propValue = await value.Get<string>(cancellationToken);
            infoConverterByPropName.SetValue(result, propName, propValue);
        }

        return result;
    }

    public static T To<T>(this NpgsqlDataReader reader)
        where T : class
    {
        var result = Activator.CreateInstance<T>();
        var type = typeof(T);
        var infoConverterByPropName = _propInfoConvertersByType.GetOrAdd(type, GeneratePropInfoConvertionsByPropName);
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var propName = reader.GetName(i);
            var propValue = reader.GetValue(i).ToString() ?? string.Empty;
            infoConverterByPropName.SetValue(result, propName, propValue);
        }

        return result;
    }

    private static void SetValue<T>(this Dictionary<string, PropInfoConverter> infoConverterByPropName, T value, string propName, string propValue)
        where T : class
    {
        if (!infoConverterByPropName.TryGetValue(propName, out var propInfoConverter))
        {
            throw new InvalidOperationException($"Property {propName} not found on type {typeof(T).Name}.");
        }

        propInfoConverter.ConvertAndSetValue(value, propValue);
    }

    private static Dictionary<string, PropInfoConverter> GeneratePropInfoConvertionsByPropName(Type type)
    {
        return type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => new PropInfoConverter(x, TypeDescriptor.GetConverter(x.PropertyType)))
            .ToDictionary(x => x.Info.Name);
    }

    private record PropInfoConverter(PropertyInfo Info, TypeConverter Converter)
    {
        public void ConvertAndSetValue(object obj, string value) => Info.SetValue(obj, Converter.ConvertFromInvariantString(value));
    }

    public static async Task<Dictionary<string, string>> ToDictionary(this InsertMessage insertMessage, CancellationToken cancellationToken)
    {
        var columnNumber = 0;
        var result = new Dictionary<string, string>();
        await foreach (var value in insertMessage.NewRow)
        {
            var columnName = insertMessage.Relation.Columns[columnNumber++].ColumnName;
            result[columnName] = await value.Get<string>(cancellationToken);
        }

        return result;
    }

    public static Dictionary<string, string> ToDictionary(this NpgsqlDataReader reader)
    {
        var result = new Dictionary<string, string>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            result[columnName] = reader.GetValue(i).ToString()!;
        }
        return result;
    }
}
