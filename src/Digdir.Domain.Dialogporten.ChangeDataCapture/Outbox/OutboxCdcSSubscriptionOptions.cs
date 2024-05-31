namespace Digdir.Domain.Dialogporten.ChangeDataCapture.Outbox;

public sealed class OutboxCdcSSubscriptionOptions
{
    public const string SectionName = "CdcSubscriptionOption";

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
    public int SnapshotSyncThreshold { get; set; } = 1000;
    public int SnapshotBatchSize { get; set; } = 1000;
}
