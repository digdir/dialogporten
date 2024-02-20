using System.Collections.ObjectModel;
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
    public async Task<OutboxMessage> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct)
    {
        var outboxMessageDictionary = await ToDictionary(insertMessage, ct);
        return ToOutboxMessage(outboxMessageDictionary);
    }

    public Task<OutboxMessage> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct)
    {
        var outboxMessageDictionary = ToDictionary(reader);
        return Task.FromResult(ToOutboxMessage(outboxMessageDictionary));
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

    private static ReadOnlyDictionary<string, string> ToDictionary(NpgsqlDataReader reader)
    {
        var result = new Dictionary<string, string>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            result[columnName] = reader.GetValue(i).ToString()!;
        }
        return result.AsReadOnly();
    }

    private static OutboxMessage ToOutboxMessage(IDictionary<string, string> dic)
    {
        return new OutboxMessage
        {
            EventId = Guid.Parse(dic[nameof(OutboxMessage.EventId)]),
            EventType = dic[nameof(OutboxMessage.EventType)],
            EventPayload = dic[nameof(OutboxMessage.EventPayload)]
        };
    }
}
