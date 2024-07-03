using Npgsql;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Mappers;

internal interface IReplicationMapper<T>
    where T : class
{
    Task<T> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct);

    Task<T> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct);
}
