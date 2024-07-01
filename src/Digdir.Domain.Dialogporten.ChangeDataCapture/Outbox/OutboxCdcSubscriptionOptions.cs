namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Outbox;

public sealed class OutboxCdcSubscriptionOptions
{
    public const string SectionName = "OutboxSubscriptionOption";

    private string _tableName = null!;

    public required string ConnectionString { get; set; }
    public required string TableName
    {
        get => _tableName;
        set
        {
            var tableName = value.Trim().ToLowerInvariant();
            PublicationName = $"{tableName}_cdc_insert_publication";
            ReplicationSlotName = $"{tableName}_cdc_replication_slot";
            _tableName = value;
        }
    }
    public string PublicationName { get; private set; } = null!;
    public string ReplicationSlotName { get; private set; } = null!;
    public int SnapshotBatchSize { get; set; } = 1000;
    public TimeSpan CheckpointSynchronizationInterval { get; set; } = TimeSpan.FromSeconds(10);
    public TimeSpan ReplicationCheckpointTimeSkew { get; set; } = TimeSpan.FromSeconds(-2);
}
