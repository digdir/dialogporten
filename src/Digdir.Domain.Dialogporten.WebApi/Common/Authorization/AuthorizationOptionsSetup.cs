using Digdir.Domain.Dialogporten.WebApi.Common.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Authorization;

internal sealed class AuthorizationOptionsSetup : IConfigureOptions<AuthorizationOptions>
{
    private readonly AuthenticationOptions _options;

    public AuthorizationOptionsSetup(IOptions<AuthenticationOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(AuthorizationOptions options)
    {
        var authenticatonSchemas = _options.JwtBearerTokenSchemas
            .Select(x => x.Name)
            .ToArray();

        // TODO: Legg på sjekk på consumer claim
        // Require "authority": "iso6523-actorid-upis",
        // Require "ID": "0192:*"
        // Require norsk org nr (mod 11, ligger i app)
        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(authenticatonSchemas)
            .Build();

        options.AddPolicy(AuthorizationPolicy.Serviceprovider, builder => builder
            .Combine(options.DefaultPolicy)
            .RequireScope("digdir:dialogporten.serviceprovider"));

        options.AddPolicy(AuthorizationPolicy.ServiceproviderSearch, builder => builder
            .Combine(options.DefaultPolicy)
            .RequireScope("digdir:dialogporten.serviceprovider.search"));
    }
}
