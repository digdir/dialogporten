namespace Digdir.Tool.Dialogporten.LargeDataSetGenerator;
#pragma warning disable CA1305

internal static class Dialog
{
    private static readonly string[] ServiceResources = File.ReadAllLines("./service_resources");
    public const string CopyCommand = """COPY "Dialog" ("Id", "CreatedAt", "Deleted", "DeletedAt", "DueAt", "ExpiresAt", "ExtendedStatus", "ExternalReference", "Org", "Party", "PrecedingProcess", "Process", "Progress", "Revision", "ServiceResource", "ServiceResourceType", "StatusId", "VisibleFrom", "UpdatedAt") FROM STDIN (FORMAT csv, HEADER false, NULL '')""";

    public static string Generate(DialogTimestamp dto)
    {
        var serviceResourceIndex = dto.Counter % ServiceResources.Length;
        var serviceResource = ServiceResources[serviceResourceIndex];
        // TODO: Party round robin
        return
            $"{dto.DialogId},{dto.FormattedTimestamp},FALSE,,,,sql-generated,NULL,ttd,partyHere,NULL,NULL,11,{Guid.NewGuid()},{serviceResource},GenericAccessResource,1,,{dto.FormattedTimestamp}";
    }
}
