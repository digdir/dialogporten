using System.Data;
using System.Runtime.CompilerServices;
using Npgsql;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;

internal static class NpgsqlDataSourceExtensions
{
    public static async Task Execute(
        this NpgsqlDataSource dataSource,
        string sql,
        CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand(sql);
        await command.ExecuteNonQueryAsync(ct);
    }

    public static async Task<bool> Exists(
        this NpgsqlDataSource dataSource,
        string table,
        string where,
        object[] parameters,
        CancellationToken ct)
    {
        await using var command = dataSource.CreateCommand($"SELECT EXISTS(SELECT 1 FROM {table} WHERE {where})");
        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(parameter);
        }

        return await command.ExecuteScalarAsync(ct) as bool? == true;
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
}
