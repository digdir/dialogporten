using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.GraphQL.Common.Extensions.HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Language;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using AuthorizationOptions = Microsoft.AspNetCore.Authorization.AuthorizationOptions;

namespace Digdir.Domain.Dialogporten.GraphQL.Common.Authorization;

internal sealed class AuthorizationOptionsSetup : IConfigureOptions<AuthorizationOptions>
{
    private readonly GraphQlSettings _options;

    public AuthorizationOptionsSetup(IOptions<GraphQlSettings> options)
    {
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

                if (authContext.Document.Definitions.Count == 0) return false;

                var definition = authContext.Document.Definitions[0];

                if (definition is not OperationDefinitionNode operationDefinition) return false;

                if (operationDefinition.Operation != OperationType.Subscription) return false;

                if (!operationDefinition.TryGetDialogEventsSubscriptionDialogId(out var dialogId))
                {
                    return false;
                }

                context.User.TryGetClaimValue(DialogTokenClaimTypes.DialogId, out var dialogIdClaimValue);
                return dialogId.ToString() == dialogIdClaimValue;
            }));
    }
}
