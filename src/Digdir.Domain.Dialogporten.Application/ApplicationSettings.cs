namespace Digdir.Domain.Dialogporten.Application;

public sealed class ApplicationSettings
{
    public const string ConfigurationSectionName = "Application";
    
    public required DialogportenSettings Dialogporten { get; init; }
}

public sealed class DialogportenSettings
{
    public required string BaseUri { get; init; }
}
