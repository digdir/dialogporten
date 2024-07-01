using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Azure.Messaging.EventGrid.SystemEvents;
using Npgsql.Replication.PgOutput.Messages;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Subscriptions;

internal interface ICdcSubscription<T>
{
    IAsyncEnumerable<IReadOnlyCollection<T>> Subscribe(CancellationToken ct);
}
