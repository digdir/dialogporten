using Digdir.Domain.Dialogporten.GraphQL.EndUser;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.DialogById;
using Digdir.Domain.Dialogporten.GraphQL.EndUser.SearchDialogs;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using HotChocolate.Subscriptions;
using StackExchange.Redis;
using Constants = Digdir.Domain.Dialogporten.Infrastructure.GraphQl.GraphQlSubscriptionConstants;

namespace Digdir.Domain.Dialogporten.GraphQL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDialogportenGraphQl(this IServiceCollection services,
        string redisConnectionString)
    {
        return services
            .AddGraphQLServer()
            .AddRedisSubscriptions(_ => ConnectionMultiplexer.Connect(redisConnectionString),
                new SubscriptionOptions
                {
                    TopicPrefix = Constants.SubscriptionTopicPrefix
                })
            .AddSubscriptionType<Subscriptions>()
            .AddAuthorization()
            .RegisterDbContext<DialogDbContext>()
            .AddDiagnosticEventListener<ApplicationInsightEventListener>()
            .AddQueryType<Queries>()
            .AddType<DialogByIdDeleted>()
            .AddType<DialogByIdNotFound>()
            .AddType<DialogByIdForbidden>()
            .AddType<SearchDialogValidationError>()
            .AddType<SearchDialogForbidden>()
            .InitializeOnStartup()
            .Services;
    }
}
