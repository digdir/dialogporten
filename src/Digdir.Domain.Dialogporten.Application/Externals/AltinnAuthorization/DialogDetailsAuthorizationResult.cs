namespace Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

public sealed class DialogDetailsAuthorizationResult
{
    // Each action applies to a resource. This is the main resource and/or one or more dialog elements.
    public Dictionary<string, List<string>> AuthorizationAttributesByAuthorizedActions { get; set; } = new();
}
