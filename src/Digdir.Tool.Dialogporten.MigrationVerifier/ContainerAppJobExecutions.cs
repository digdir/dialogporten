using System.Text.Json.Serialization;
// ReSharper disable ClassNeverInstantiated.Global

namespace Digdir.Tool.Dialogporten.MigrationVerifier;

public class Container
{
    [JsonPropertyName("image")]
    public string Image { get; set; } = null!;
}

public class Properties
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = null!;

    [JsonPropertyName("template")]
    public Template Template { get; set; } = null!;
}

public class ContainerAppJobExecutions
{
    [JsonPropertyName("value")]
    public List<Executions> Executions { get; set; } = null!;
}

public class Template
{
    [JsonPropertyName("containers")]
    public List<Container> Containers { get; set; } = null!;
}

public class Executions
{
    [JsonPropertyName("properties")]
    public Properties Properties { get; set; } = null!;
}


