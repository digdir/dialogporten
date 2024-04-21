using Digdir.Domain.Dialogporten.GraphQL.EndUser;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;

namespace Digdir.Domain.Dialogporten.GraphQL;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDialogportenGraphQl(
        this IServiceCollection services)
    {
        return services
            .AddGraphQLServer()
            .AddAuthorization()
            .RegisterDbContext<DialogDbContext>()
            .AddDiagnosticEventListener<ApplicationInsightEventListener>()
            .AddQueryType<DialogQueries>()
            .Services;
    }
}
