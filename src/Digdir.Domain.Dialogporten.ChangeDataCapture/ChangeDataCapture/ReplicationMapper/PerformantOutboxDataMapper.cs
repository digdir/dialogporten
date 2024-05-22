using System.Globalization;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.ReplicationMapper;

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

    public static async Task<Dictionary<string, string>> ToDictionary(InsertMessage insertMessage, CancellationToken cancellationToken)
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

    public static Dictionary<string, string> ToDictionary(NpgsqlDataReader reader)
    {
        var result = new Dictionary<string, string>();
        for (var i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            result[columnName] = reader.GetValue(i).ToString()!;
        }
        return result;
    }

    private static OutboxMessage ToOutboxMessage(IReadOnlyDictionary<string, string> dic)
    {
        return new OutboxMessage
        {
            EventId = Guid.Parse(dic[nameof(OutboxMessage.EventId)]),
            CreatedAt = DateTimeOffset.Parse(dic[nameof(OutboxMessage.CreatedAt)], CultureInfo.InvariantCulture),
            EventType = dic[nameof(OutboxMessage.EventType)],
            EventPayload = dic[nameof(OutboxMessage.EventPayload)]
        };
    }
}
