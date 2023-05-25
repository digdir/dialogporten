using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Service;
using RabbitMQ.Client;

// TODO: Add application insights and serilog logging with two stage initialization
// TODO: Add AppConfiguration and keyvault
// TODO: Configure RabbitMQ connection settings 
// TODO: Configure Postgres connection settings
// TODO: Improve exceptions thrown in this assembly


var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddHostedService<RabbitMqSubscriber>()
    .AddSingleton<IConnection>(x => new ConnectionFactory
    {
        ClientProvidedName = "Digdir.Domain.Dialogporten.Service",
        DispatchConsumersAsync = true,
    }.CreateConnection())
    .AddSingleton<IModel>(x =>
    {
        const uint Unlimited = 0;
        const ushort PrefetchCount = 100;

        var channel = x.GetRequiredService<IConnection>().CreateModel();
        channel.BasicQos(
            prefetchSize: Unlimited,
            prefetchCount: PrefetchCount,
            global: false);
        return channel;
    })
    .AddSingleton<RabbitMqSubscription>()
    .AddApplication(builder.Configuration.GetSection(ApplicationSettings.ConfigurationSectionName))
    .AddInfrastructure(builder.Configuration.GetSection(InfrastructureSettings.ConfigurationSectionName));

var app = builder.Build();
app.UseHttpsRedirection();
app.Run();


