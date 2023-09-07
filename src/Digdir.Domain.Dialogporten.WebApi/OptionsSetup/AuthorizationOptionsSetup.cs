using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.WebApi.OptionsSetup;

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

        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(authenticatonSchemas)
            .Build();

        options.AddPolicy(Policy.Serviceprovider, builder => builder
            .Combine(options.DefaultPolicy)
            .RequireAssertion(ctx => ctx.User.Identities
                .SelectMany(x => x.Claims)
                .Where(x => x.Type == "scope")
                .SelectMany(x => x.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Any(x => x == "digdir:dialogporten.serviceprovider")));
    }
}
