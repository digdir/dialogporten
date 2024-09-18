using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.GraphQL.Common.Extensions.HotChocolate;
using HotChocolate.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using AuthorizationOptions = Microsoft.AspNetCore.Authorization.AuthorizationOptions;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;

internal sealed class AuthorizationOptionsSetup : IConfigureOptions<AuthorizationOptions>
{
    private readonly GraphQlSettings _options;
    private readonly ICompactJwsGenerator _compactJwsGenerator;
    public const string DialogTokenHeader = "DigDir-Dialog-Token";

    public AuthorizationOptionsSetup(IOptions<GraphQlSettings> options, ICompactJwsGenerator compactJwsGenerator)
    {
        _compactJwsGenerator = compactJwsGenerator ?? throw new ArgumentNullException(nameof(compactJwsGenerator));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public void Configure(AuthorizationOptions options)
    {
        var authenticationSchemas = _options
            .Authentication
            .JwtBearerTokenSchemas
            .Select(x => x.Name)
            .ToArray();

        options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes(authenticationSchemas)
            .RequireValidConsumerClaim()
            .Build();

        options.AddPolicy(AuthorizationPolicy.EndUser, builder => builder
            .Combine(options.DefaultPolicy)
            .RequireScope(AuthorizationScope.EndUser));

        options.AddPolicy(AuthorizationPolicy.ServiceProvider, builder => builder
            .Combine(options.DefaultPolicy)
            .RequireScope(AuthorizationScope.ServiceProvider));

        options.AddPolicy(AuthorizationPolicy.ServiceProviderSearch, builder => builder
            .Combine(options.DefaultPolicy)
            .RequireScope(AuthorizationScope.ServiceProviderSearch));

        options.AddPolicy(AuthorizationPolicy.Testing, builder => builder
            .Combine(options.DefaultPolicy)
            .RequireScope(AuthorizationScope.Testing));

        options.AddPolicy(AuthorizationPolicy.EndUserSubscription, policy => policy
            .Combine(options.GetPolicy(AuthorizationPolicy.EndUser)!)
            .RequireAssertion(context =>
            {
                if (context.Resource is not AuthorizationContext authContext)
                {
                    return false;
                }

                if (!authContext.ContextData.TryGetValue("HttpContext", out var httpContextObj))
                {
                    return false;
                }

                if (httpContextObj is not HttpContext httpContext)
                {
                    return false;
                }

                if (!authContext.Document.Definitions.TryGetSubscriptionDialogId(out var dialogId))
                {
                    return false;
                }

                if (!httpContext.Request.Headers.TryGetValue(DialogTokenHeader, out var dialogToken))
                {
                    return false;
                }

                if (string.IsNullOrWhiteSpace(dialogToken))
                {
                    return false;
                }

                if (!_compactJwsGenerator.VerifyCompactJws(dialogToken!))
                {
                    return false;
                }

                if (!_compactJwsGenerator.VerifyCompactJwsTimestamp(dialogToken!))
                {
                    return false;
                }

                if (!_compactJwsGenerator.TryGetClaimValue(dialogToken!, DialogTokenClaimTypes.DialogId, out var dialogTokenDialogId))
                {
                    return false;
                }

                if (dialogId.ToString() != dialogTokenDialogId)
                {
                    return false;
                }

                return true;
            }));
    }
}
