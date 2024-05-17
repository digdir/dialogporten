using System.Data;
using System.Runtime.CompilerServices;
using Npgsql;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;

internal static class NpgsqlDataSourceExtensions
{
    public static async Task EnsureInsertPublicationForTable(this NpgsqlDataSource dataSource, string tableName, CancellationToken ct)
    {
        var publicationName = GetPublicationNameForTable(tableName);
        await using var command = dataSource.CreateCommand(
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

    public static async Task<bool> ReplicationSlotExists(
        this NpgsqlDataSource dataSource,
        string slotName,
        CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand("SELECT EXISTS(SELECT 1 FROM pg_replication_slots WHERE slot_name = $1)");
        command.Parameters.AddWithValue(slotName);
        return (await command.ExecuteScalarAsync(ct) as bool?) == true;
    }

    public static async Task DropReplicationSlot(
        this NpgsqlDataSource dataSource,
        string slotName,
        CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand(
            $"""
            SELECT pg_drop_replication_slot('{slotName}') 
            FROM pg_replication_slots WHERE slot_name = '{slotName}';
            """);
        await command.ExecuteNonQueryAsync(ct);
    }

    public static async IAsyncEnumerable<NpgsqlDataReader> ReadExistingRowsFromSnapshot(
        this NpgsqlDataSource dataSource,
        string snapshotName,
        string tableName,
        [EnumeratorCancellation] CancellationToken ct)
    {
        await using var connection = await dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead, ct);

        await using var command = new NpgsqlCommand($"SET TRANSACTION SNAPSHOT '{snapshotName}';", connection, transaction);
        await command.ExecuteScalarAsync(ct);

        await using var cmd = new NpgsqlCommand($"""SELECT * FROM "{tableName}" """, connection, transaction);
        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            yield return reader;
        }
    }

    private static string GetPublicationNameForTable(string tableName) => $"{tableName.Trim().ToLowerInvariant()}_insert_publication";

    //public static async Task Execute(
    //    this NpgsqlDataSource dataSource,
    //    string sql,
    //    CancellationToken ct)
    //{
    //    await using var command = dataSource.CreateCommand(sql);
    //    await command.ExecuteNonQueryAsync(ct);
    //}

    //public static async Task<bool> InsertPublicationExists(
    //    this NpgsqlDataSource dataSource,
    //    string tableName,
    //    CancellationToken ct)
    //{
    //    await using var command = dataSource.CreateCommand("SELECT EXISTS(SELECT 1 FROM pg_publication WHERE pubname = $1)");
    //    command.Parameters.AddWithValue(GetPublicationNameForTable(tableName));
    //    return (await command.ExecuteScalarAsync(ct) as bool?) == true;
    //}

    //public static async Task<bool> CreateInsertPublication(
    //    this NpgsqlDataSource dataSource,
    //    string tableName,
    //    CancellationToken ct)
    //{
    //    var publicationName = GetPublicationNameForTable(tableName);
    //    await using var command = dataSource.CreateCommand($"""CREATE PUBLICATION {publicationName} FOR TABLE "{tableName}" WITH (publish = 'insert', publish_via_partition_root = false);""");
    //    command.Parameters.AddWithValue(publicationName);
    //    return (await command.ExecuteScalarAsync(ct) as bool?) == true;
    //}
}
