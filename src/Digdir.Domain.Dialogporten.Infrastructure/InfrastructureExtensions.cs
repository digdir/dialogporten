using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents.Outbox.Dispatcher;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configurationSection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurationSection);
        return services
            // Settings
            .Configure<InfrastructureSettings>(configurationSection)

            // Framework
            .AddDbContext<DialogueDbContext>((services, options) =>
            {
                var connectionString = services
                    .GetRequiredService<IOptions<InfrastructureSettings>>()
                    .Value.DialogueDbConnectionString;
                options.UseNpgsql(connectionString)
                    .AddInterceptors(services.GetRequiredService<ConvertDomainEventsToOutboxMessagesInterceptor>());
            })
            .AddHostedService<DevelopmentMigratorHostedService>()
            .AddHostedService<OutboxScheduler>()

            // Singleton

            // Scoped
            .AddScoped<ConvertDomainEventsToOutboxMessagesInterceptor>()
            .AddScoped<DomainEventPublisher>()
            .AddScoped<IDomainEventPublisher>(x => x.GetRequiredService<DomainEventPublisher>())
            .AddScoped<IDialogueDbContext>(x => x.GetRequiredService<DialogueDbContext>())
            .AddScoped<IUnitOfWork, UnitOfWork>()

            // Transient
            .AddTransient<OutboxDispatcher>()
            
            // Decorate
            .Decorate(typeof(INotificationHandler<>), typeof(IdempotentDomainEventHandler<>));
    }
}
