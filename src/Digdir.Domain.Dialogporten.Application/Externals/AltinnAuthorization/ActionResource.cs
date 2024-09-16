using Digdir.Domain.Dialogporten.Application.Common.Authorization;

namespace Digdir.Domain.Dialogporten.Application.Externals.AltinnAuthorization;

public sealed record AltinnAction
{
    public string Name { get; }
    public string AuthorizationAttribute { get; }

    public AltinnAction(string name, string? authorizationAttribute = null)
    {
        Name = name;
        AuthorizationAttribute = authorizationAttribute ?? Constants.MainResource;
    }

    public void Deconstruct(out string key, out string value)
        => (key, value) = (Name, AuthorizationAttribute);
}
