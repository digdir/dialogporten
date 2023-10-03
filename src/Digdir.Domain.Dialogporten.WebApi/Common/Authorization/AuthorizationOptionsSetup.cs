﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.WebApi.Common.Authorization;

internal sealed class AuthorizationOptionsSetup : IConfigureOptions<AuthorizationOptions>
{
    private readonly WebApiOptions _options;

    public AuthorizationOptionsSetup(IOptions<WebApiOptions> options)
    {
        _options = options.Value;
    }

    public void Configure(AuthorizationOptions options)
    {
        var authenticatonSchemas = _options
            .Authentication
            .JwtBearerTokenSchemas
            .Select(x => x.Name)
            .ToArray();

        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(authenticatonSchemas)
            .RequireValidConsumerClaim()
            .Build();

        options.AddPolicy(AuthorizationPolicy.Serviceprovider, builder => builder
            .Combine(options.DefaultPolicy)
            .RequireScope("digdir:dialogporten.serviceprovider"));

        options.AddPolicy(AuthorizationPolicy.ServiceproviderSearch, builder => builder
            .Combine(options.DefaultPolicy)
            .RequireScope("digdir:dialogporten.serviceprovider.search"));
    }
}
