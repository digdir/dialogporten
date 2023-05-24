using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;

public interface IJsonReplicationDataMapper
{
    Task<object> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct);

    Task<object> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct);
}

public class JsonReplicationDataMapper : IJsonReplicationDataMapper
{
    private static readonly Assembly _eventAssembly = typeof(OutboxMessage).Assembly;
    private static readonly ConcurrentDictionary<string, Type?> _typeCache = new();

    public async Task<object> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct)
    {
        var outboxMessageDictionary = await ToDictionary(insertMessage, ct);
        var @event = GetEvent(outboxMessageDictionary);
        return @event;
    }

    public Task<object> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct)
    {
        var outboxMessageDictionary = ToDictionary(reader);
        var @event = GetEvent(outboxMessageDictionary);
        return Task.FromResult(@event);
    }

    private static async Task<IDictionary<string, string>> ToDictionary(InsertMessage insertMessage, CancellationToken cancellationToken)
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

    private static IDictionary<string, string> ToDictionary(NpgsqlDataReader reader)
    {
        var result = new Dictionary<string, string>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            result[columnName] = reader.GetValue(i).ToString()!;
        }
        return result;
    }

    private static object GetEvent(IDictionary<string, string> outboxMessageDictionary)
    {
        var eventTypeName = outboxMessageDictionary[nameof(OutboxMessage.EventType)];
        var eventPayload = outboxMessageDictionary[nameof(OutboxMessage.EventPayload)];

        if (!TryGetType(eventTypeName, out var eventType))
        {
            // TODO: Improve exception
            throw new InvalidOperationException();
        }

        var @event = JsonSerializer.Deserialize(eventPayload, eventType);
        if (@event is null)
        {
            // TODO: Improve exception
            throw new ApplicationException();
        }

        return @event;
    }

    private static bool TryGetType(string? typeName, [NotNullWhen(true)] out Type? type)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            type = null;
            return false;
        }

        type = _typeCache.GetOrAdd(typeName, _eventAssembly.GetType(typeName));
        return type is not null;
    }
}