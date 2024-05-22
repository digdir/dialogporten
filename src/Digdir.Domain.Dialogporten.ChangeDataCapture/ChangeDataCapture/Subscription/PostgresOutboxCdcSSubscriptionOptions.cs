namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Subscription;

public record PostgresOutboxCdcSSubscriptionOptions(
    string ConnectionString,
    string TableName,
    string PublicationName = null!,
    string ReplicationSlotName = null!,
    int SnapshotSyncThreshold = 1000)
{
    public const string SectionName = "CdcSubscriptionOption";
    public string PublicationName { get; init; } = PublicationName ?? $"{TableName.Trim().ToLowerInvariant()}_insert_publication";
    public string ReplicationSlotName { get; init; } = ReplicationSlotName ?? $"{TableName.Trim().ToLowerInvariant()}_cdc_replication_slot";
}
