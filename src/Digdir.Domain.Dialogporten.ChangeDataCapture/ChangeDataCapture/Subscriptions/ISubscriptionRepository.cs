using Npgsql;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture.Subscriptions;

internal interface ISubscriptionRepository
{
    Task DropReplicationSlot(string slotName, CancellationToken ct = default);
    Task EnsureInsertPublicationForTable(string tableName, string publicationName, CancellationToken ct = default);
    Task<bool> ReplicationSlotExists(string slotName, CancellationToken ct = default);
}

internal sealed class SubscriptionRepository : ISubscriptionRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public SubscriptionRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
    }

    public async Task EnsureInsertPublicationForTable(
        string tableName,
        string publicationName,
        CancellationToken ct = default)
    {
        await using var command = _dataSource.CreateCommand(
            $"""
            DO
            $do$
            BEGIN
                IF NOT EXISTS(
                    SELECT 1 
                    FROM pg_publication 
                    WHERE pubname = '{publicationName}') 
                THEN
                    CREATE PUBLICATION {publicationName} 
                    FOR TABLE "{tableName}" 
                    WITH (publish = 'insert', publish_via_partition_root = false);
                END IF;
            END
            $do$;
            """);
        await command.ExecuteNonQueryAsync(ct);
    }

    public async Task<bool> ReplicationSlotExists(
        string slotName,
        CancellationToken ct = default)
    {
        await using var command = _dataSource.CreateCommand("SELECT EXISTS(SELECT 1 FROM pg_replication_slots WHERE slot_name = $1)");
        command.Parameters.AddWithValue(slotName);
        return (await command.ExecuteScalarAsync(ct) as bool?) == true;
    }

    public async Task DropReplicationSlot(
        string slotName,
        CancellationToken ct = default)
    {
        await using var command = _dataSource.CreateCommand($"""
            SELECT pg_drop_replication_slot('{slotName}') 
            FROM pg_replication_slots WHERE slot_name = '{slotName}';
            """);
        await command.ExecuteNonQueryAsync(ct);
    }
}
