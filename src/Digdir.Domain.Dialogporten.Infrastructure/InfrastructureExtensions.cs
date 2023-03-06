using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Action<InfrastructureSettings>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        if (configure is not null)
        {
            services.Configure(configure);
        }

        return services

            // Framework
            .AddDbContext<DialogueDbContext>((services, options) =>
            {
                var connectionString = services
                    .GetRequiredService<IOptions<InfrastructureSettings>>()
                    .Value.DialogueDbConnectionString;
                var interceptor = services
                    .GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>();
                options.UseNpgsql(connectionString).AddInterceptors(interceptor);
            })
            .AddHostedService<DevelopmentMigratorHostedService>()

            // Singleton
            .AddSingleton<ConvertDomainEventsToOutboxMessagesInterceptor>()

            // Scoped
            .AddScoped<IDialogueDbContext>(x => x.GetRequiredService<DialogueDbContext>())
            .AddScoped<IUnitOfWork, UnitOfWork>()
            
            // Decorate
            .Decorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandler<>));
    }
}
