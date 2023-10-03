using FluentValidation;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Options;

public sealed class AuthenticationOptions
{
    public required List<JwtBearerTokenSchemasOptions> JwtBearerTokenSchemas { get; init; }
}

public sealed class JwtBearerTokenSchemasOptions
{
    public required string Name { get; init; }
    public required string WellKnown { get; init; }
}