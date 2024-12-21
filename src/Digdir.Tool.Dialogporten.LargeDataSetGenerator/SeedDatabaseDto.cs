using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;

#pragma warning disable CA1305
internal sealed class SeedDatabaseDto
{
    public SeedDatabaseDto(DateTimeOffset fromDate, DateTimeOffset toDate, int dialogAmount)
    {
        Interval = TimeSpan.FromTicks((toDate.Ticks - fromDate.Ticks) / dialogAmount);
        DialogAmount = dialogAmount;
        FromDate = fromDate;
        ToDate = toDate;
    }

    public IEnumerable<DialogTimestamp> GetDialogTimestamps =>
        Enumerable.Range(0, DialogAmount)
            .Select(x =>
            {
                var timestamp = FromDate + (Interval * x);
                var formattedTimestamp = timestamp.ToString("yyyy-MM-dd HH:mm:ss zzz");
                var dialogId = DeterministicUuidV7.Generate(timestamp, nameof(DialogEntity));
                var counter = x + 1;
                return new DialogTimestamp(timestamp, formattedTimestamp, dialogId, counter);
            });
    public DateTimeOffset FromDate { get; }
    public DateTimeOffset ToDate { get; }
    public TimeSpan Interval { get; }
    public int DialogAmount { get; }
}
