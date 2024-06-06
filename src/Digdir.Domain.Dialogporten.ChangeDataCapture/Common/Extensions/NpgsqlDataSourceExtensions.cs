using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Npgsql;
using Npgsql.Replication;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Common.Extensions;

internal static class NpgsqlDataSourceExtensions
{
    public static async Task AcknowledgeWalMessage(
        this LogicalReplicationConnection replicationConnection,
        PgOutputReplicationMessage message,
        CancellationToken ct = default)
    {
        // Always call SetReplicationStatus() or assign LastAppliedLsn and LastFlushedLsn individually
        // so that Npgsql can inform the server which WAL files can be removed/recycled.
        replicationConnection.SetReplicationStatus(message.WalEnd);
        await replicationConnection.SendStatusUpdate(ct);
    }
}
