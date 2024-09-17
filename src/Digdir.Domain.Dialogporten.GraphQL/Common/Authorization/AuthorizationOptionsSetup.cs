using System.Security.Claims;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.GraphQL.Common.Extensions.HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using AuthorizationOptions = Microsoft.AspNetCore.Authorization.AuthorizationOptions;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;

internal sealed class AuthorizationOptionsSetup : IConfigureOptions<AuthorizationOptions>
{
    private readonly GraphQlSettings _options;
    private readonly ICompactJwsGenerator _compactJwsGenerator;
    private const string DialogTokenHeader = "DigDir-Dialog-Token";

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
                // Cast resource to MiddleWareContext
                // Get first value from FieldSelection.Arguments (DialogIdSubscriptionInput)
                // Get HttpContext from context.Resource.ContextData, key is "HttpContext" and assign to var httpContext
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
                    // requestBuilder.SetQuery("");
                    // const string message = "{\"errors\": [{\"message\": \"Forbidden, missing header 'DigDir-Dialog-Token'\"}]}";
                    // await SendForbiddenAsync(context, message, cancellationToken);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(dialogToken))
                {
                    // requestBuilder.SetQuery("");
                    // const string message = "{\"errors\": [{\"message\": \"Forbidden, empty token\"}]}";
                    // await SendForbiddenAsync(context, message, cancellationToken);
                    return false;
                }

                if (!_compactJwsGenerator.VerifyCompactJws(dialogToken!))
                {
                    // requestBuilder.SetQuery("");
                    // const string message = "{\"errors\": [{\"message\": \"Forbidden, invalid token\"}]}";
                    // await SendForbiddenAsync(context, message, cancellationToken);
                    return false;
                }

                if (!_compactJwsGenerator.VerifyCompactJwsTimestamp(dialogToken!))
                {
                    // requestBuilder.SetQuery("");
                    // const string message = "{\"errors\": [{\"message\": \"Forbidden, expired token\"}]}";
                    // await SendForbiddenAsync(context, message, cancellationToken);
                    return false;
                }

                if (!_compactJwsGenerator.TryGetClaimValue(dialogToken!, "i", out var dialogTokenDialogId))
                {
                    // requestBuilder.SetQuery("");
                    // const string message = "{\"errors\": [{\"message\": \"Forbidden, missing claim 'i', (DialogId)\"}]}";
                    // await SendForbiddenAsync(context, message, cancellationToken);
                    return false;
                }

                if (dialogId.ToString() != dialogTokenDialogId)
                {
                    // requestBuilder.SetQuery("");
                    // const string message = "{\"errors\": [{\"message\": \"Forbidden, token dialogId does not match subscription dialogId\"}]}";
                    // await SendForbiddenAsync(context, message, cancellationToken);
                    return false;
                }


                return true;
            }));
    }
}
