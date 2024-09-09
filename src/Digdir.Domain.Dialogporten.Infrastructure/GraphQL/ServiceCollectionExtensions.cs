using HotChocolate.Execution.Configuration;
using HotChocolate.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Digdir.Domain.Dialogporten.Infrastructure.GraphQl;

/// <summary>
/// This implementation is a workaround to allow the use of the AddRedisSubscriptions extension method
/// from HotChocolate.Subscriptions.Redis without having to take the entire HotChocolate library as a dependency.
/// </summary>
internal sealed class DummyRequestExecutorBuilder : IRequestExecutorBuilder
{
    public string Name => string.Empty;
    public IServiceCollection Services { get; init; } = null!;
}

public static class GraphQlSubscriptionConstants
{
    public const string SubscriptionTopicPrefix = "graphql_subscriptions_";
    public const string DialogUpdatedTopic = "dialogUpdated/";
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGraphQlRedisSubscriptions(this IServiceCollection services,
        string redisConnectionString)
    {
        var dummyImplementation = new DummyRequestExecutorBuilder { Services = services };
        dummyImplementation.AddRedisSubscriptions(_ => ConnectionMultiplexer.Connect(redisConnectionString),
            new SubscriptionOptions
            {
                TopicPrefix = GraphQlSubscriptionConstants.SubscriptionTopicPrefix
            });

        return services;
    }
}
