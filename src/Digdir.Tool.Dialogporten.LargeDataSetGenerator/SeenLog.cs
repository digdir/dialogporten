namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class SeenLog
{
    public const string CopyCommand = """COPY "DialogSeenLog" ("Id", "CreatedAt", "IsViaServiceOwner", "DialogId", "EndUserTypeId") FROM STDIN (FORMAT csv, HEADER false, NULL '')""";

    public static string Generate(DialogTimestamp dto)
        => $"{dto.DialogId},{dto.FormattedTimestamp},FALSE,{dto.DialogId},1";
}
