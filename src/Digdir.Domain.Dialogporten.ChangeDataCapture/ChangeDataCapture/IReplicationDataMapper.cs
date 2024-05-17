using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Digdir.Domain.Dialogporten.Domain.Outboxes;
using MassTransit.Initializers.TypeConverters;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;

public interface IReplicationDataMapper<T>
    where T : class
{
    Task<T> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct);

    Task<T> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct);
}

internal sealed class PerformantOutboxDataMapper : IReplicationDataMapper<OutboxMessage>
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

internal sealed class DynamicReplicationDataMapper<T> : IReplicationDataMapper<T>
    where T : class
{
    public Task<T> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct) => insertMessage.To<T>(ct);
    public Task<T> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct) => Task.FromResult(reader.To<T>());
}
