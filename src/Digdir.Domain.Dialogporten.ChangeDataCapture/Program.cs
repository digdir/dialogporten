using Digdir.Domain.Dialogporten.ChangeDataCapture;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;
using Digdir.Domain.Dialogporten.ChangeDataCapture.Sinks;

// TODO: Add application insights and serilog logging with two stage initialization
// TODO: Add AppConfiguration and keyvault
// TODO: Configure RabbitMQ connection settings 
// TODO: Configure Postgres connection settings
// TODO: Improve exceptions thrown in this assembly

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddHostedService<CdcBackgroundHandler>()
    .AddSingleton(x => new PostgresCdcSSubscriptionOptions
        (
            ConnectionString: builder.Configuration["Infrastructure:DialogueDbConnectionString"]!,
            ReplicationSlotName: builder.Configuration["ReplicationSlotName"]!,
            PublicationName: builder.Configuration["PublicationName"]!,
            TableName: builder.Configuration["TableName"]!,
            DataMapper: new JsonReplicationDataMapper()
        ))
    .AddTransient<IPostgresCdcSubscription, PostgresCdcSubscription>()
    .AddSingleton<ISink, RabbitMqSink>();

var app = builder.Build();
app.UseHttpsRedirection();
app.Run();
