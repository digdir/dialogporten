namespace Digdir.Domain.Dialogporten.WebApi.OptionsSetup;

public sealed class AuthenticationOptions
{
    public const string SectionName = "Authentication";

    public required List<JwtBearerTokenSchemasOptions> JwtBearerTokenSchemas { get; init; }
}

public sealed class JwtBearerTokenSchemasOptions
{
    public required string Name { get; init; }
    public required string WellKnown { get; init; }
}
