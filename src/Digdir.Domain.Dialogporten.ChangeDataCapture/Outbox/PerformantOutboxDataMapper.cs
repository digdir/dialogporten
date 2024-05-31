using System.Globalization;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.ReplicationMapper;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Outbox;

internal sealed class PerformantOutboxDataMapper : IReplicationDataMapper<OutboxMessage>
{
    public Task<OutboxMessage> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct)
    {
        var outboxMessageDictionary = ToDictionary(reader);
        return Task.FromResult(ToOutboxMessage(outboxMessageDictionary));
    }

    public async Task<OutboxMessage> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct)
    {
        var outboxMessageDictionary = await ToDictionary(insertMessage, ct);
        return ToOutboxMessage(outboxMessageDictionary);
    }

    public static async Task<Dictionary<string, object?>> ToDictionary(InsertMessage insertMessage, CancellationToken cancellationToken)
    {
        var columnNumber = 0;
        var result = new Dictionary<string, object?>();
        await foreach (var value in insertMessage.NewRow)
        {
            var columnName = insertMessage.Relation.Columns[columnNumber++].ColumnName;
            result[columnName] = await value.Get(cancellationToken);
        }

        return result;
    }

    public static Dictionary<string, object?> ToDictionary(NpgsqlDataReader reader)
    {
        var result = new Dictionary<string, object?>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            result[columnName] = reader.GetValue(i);
        }
        return result;
    }

    private static OutboxMessage ToOutboxMessage(IReadOnlyDictionary<string, object?> dic)
    {
        return new OutboxMessage
        {
            EventId = (Guid)dic[nameof(OutboxMessage.EventId)]!,
            CreatedAt = (DateTimeOffset)(DateTime)dic[nameof(OutboxMessage.CreatedAt)]!,
            EventType = (string)dic[nameof(OutboxMessage.EventType)]!,
            EventPayload = (string)dic[nameof(OutboxMessage.EventPayload)]!
        };
    }
}
