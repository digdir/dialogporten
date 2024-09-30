using System.Reflection;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Digdir.Domain.Dialogporten.Infrastructure;

public interface IInfrastructureBuilder
{
    IServiceCollection Build();
}

public interface IPubSubInfrastructureChoice
{
    IInfrastructureBuilder WithPubSubCapabilities(params Assembly[] consumerAssemblies);
    IInfrastructureBuilder WithoutPubSubCapabilities();
    IInfrastructureBuilder WithPubCapabilities();
}

internal sealed class InfrastructureBuilder(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment) :
    IInfrastructureBuilder,
    IPubSubInfrastructureChoice
{
    private readonly IServiceCollection _services = services ?? throw new ArgumentNullException(nameof(services));
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly IHostEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    private readonly List<Action<IServiceCollection, IConfiguration, IHostEnvironment>> _actions = [];

    public IServiceCollection Build()
    {
        _services.AddInfrastructure_Internal(_configuration, _environment);
        foreach (var action in _actions)
        {
            action(_services, _configuration, _environment);
        }
        return _services;
    }

    public IInfrastructureBuilder WithPubSubCapabilities(params Assembly[] consumerAssemblies)
        => AddAction((services, configuration, environment) => AddPubSubCapabilities(services, configuration, environment, consumerAssemblies));
    public IInfrastructureBuilder WithPubCapabilities() => AddAction(AddPubCapabilities);
    public IInfrastructureBuilder WithoutPubSubCapabilities() => this;

    private static void AddPubSubCapabilities(
        IServiceCollection services,
        IConfiguration _,
        IHostEnvironment environment,
        Assembly[] consumerAssemblies) =>
        services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<DialogDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox();
                });

                x.AddConsumers(consumerAssemblies
                    .DefaultIfEmpty(Assembly.GetExecutingAssembly())
                    .ToArray());

                if (environment.IsDevelopment())
                {
                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.ConfigureEndpoints(context);
                    });
                    return;
                }

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            })
            .AddTransient<Lazy<IPublishEndpoint>>(x => new Lazy<IPublishEndpoint>(x.GetRequiredService<IPublishEndpoint>));
    // Magnus: Legg til dette?
    // .AddGraphQlRedisSubscriptions();

    private static void AddPubCapabilities(IServiceCollection services, IConfiguration configuration,
        IHostEnvironment environment) =>
        services.AddMassTransit(x =>
            {
                x.AddEntityFrameworkOutbox<DialogDbContext>(o =>
                {
                    o.UsePostgres();
                    o.UseBusOutbox(y => y.DisableDeliveryService());
                });

                if (environment.IsDevelopment())
                {
                    x.UsingInMemory();
                    return;
                }

                x.UsingAzureServiceBus();
            })
            .RemoveHostedService<InboxCleanupService<DialogDbContext>>()
            .AddTransient<Lazy<IPublishEndpoint>>(x => new Lazy<IPublishEndpoint>(x.GetRequiredService<IPublishEndpoint>));
    // Magnus: Legg til dette?
    // .AddGraphQlRedisSubscriptions();

    private InfrastructureBuilder AddAction(Action<IServiceCollection, IConfiguration, IHostEnvironment> action)
    {
        _actions.Add(action);
        return this;
    }
}
