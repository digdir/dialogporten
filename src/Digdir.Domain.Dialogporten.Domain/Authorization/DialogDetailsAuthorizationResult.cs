namespace Digdir.Domain.Dialogporten.Domain.Authorization;


public sealed class DialogDetailsAuthorizationResult
{
    public const string MainResource = "main";
    public const string ElementReadAction = "read";

    // Each action applies to a resource. This is the main resource and/or one or more dialog elements.
    public Dictionary<string, List<string>> AuthorizedActions { get; set; } = new();
}
