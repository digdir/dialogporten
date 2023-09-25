namespace Digdir.Domain.Dialogporten.Application;

public sealed class ApplicationSettings
{
    public const string ConfigurationSectionName = "Application";
    
    public DialogportenSettings Dialogporten { get; set; } = new();
}

public sealed class DialogportenSettings
{
    public string BaseUrl { get; set; } = string.Empty;
}
