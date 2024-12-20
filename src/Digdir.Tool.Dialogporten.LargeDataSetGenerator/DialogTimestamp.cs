namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;

public record struct DialogTimestamp(DateTimeOffset Timestamp, Guid DialogId, int Counter);
