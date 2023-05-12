using Npgsql;
using Npgsql.Replication.PgOutput.Messages;
using System.Text.Json;

namespace Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;

public interface IJsonReplicationDataMapper
{
    Task<string> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct);

    Task<string> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct);
}

public class JsonReplicationDataMapper : IJsonReplicationDataMapper
{
    public async Task<string> ReadFromReplication(InsertMessage insertMessage, CancellationToken ct)
    {
        var outboxMessageDictionary = await ToDictionary(insertMessage, ct);
        return JsonSerializer.Serialize(outboxMessageDictionary);
    }

    public Task<string> ReadFromSnapshot(NpgsqlDataReader reader, CancellationToken ct)
    {
        var outboxMessageDictionary = ToDictionary(reader);
        return Task.FromResult(JsonSerializer.Serialize(outboxMessageDictionary));
    }

    private static async Task<IDictionary<string, object>> ToDictionary(InsertMessage insertMessage, CancellationToken cancellationToken)
    {
        var columnNumber = 0;
        var result = new Dictionary<string, object>();
        await foreach (var value in insertMessage.NewRow)
        {
            var columnName = insertMessage.Relation.Columns[columnNumber++].ColumnName;
            var columnValue = await value.Get<string>(cancellationToken);
            result[columnName] = value.GetDataTypeName().ToLower() == "jsonb"
                ? JsonSerializer.Deserialize<object>(columnValue)!
                : columnValue;
        }

        return result;
    }

    private static IDictionary<string, object> ToDictionary(NpgsqlDataReader reader)
    {
        var result = new Dictionary<string, object>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            var columnValue = reader.GetValue(i).ToString()!;
            result[columnName] = reader.GetDataTypeName(i).ToLower() == "jsonb"
                ? JsonSerializer.Deserialize<object>(columnValue)!
                : columnValue;
        }
        return result;
    }
}