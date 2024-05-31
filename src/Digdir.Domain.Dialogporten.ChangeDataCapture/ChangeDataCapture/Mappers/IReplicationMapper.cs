using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using MassTransit.Initializers.TypeConverters;
using Npgsql;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.ReplicationMapper;

internal interface IReplicationMapper<T>
    where T : class
{
    Task<T> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct);

    Task<T> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct);
}
