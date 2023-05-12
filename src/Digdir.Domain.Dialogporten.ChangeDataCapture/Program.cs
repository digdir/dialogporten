using Azure.Storage.Queues;
using Digdir.Domain.Dialogporten.ChangeDataCapture.ChangeDataCapture;
using Microsoft.Extensions.Configuration;
using System.Reflection;

// TODO: Add application insights and logging
// TODO: Add AppConfiguration and keyvault

var cts = new CancellationTokenSource();
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{environment}.json", true, true)
    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
    .Build();

//var services = new ServiceCollection()
//    .AddSingleton<IConfiguration>(configuration)
//    .BuildServiceProvider();

var subscriptionOptions = new SubscriptionOptions(
    ConnectionString: configuration["Infrastructure:DialogueDbConnectionString"]!,
    ReplicationSlotName: configuration["ReplicationSlotName"]!,
    PublicationName: configuration["PublicationName"]!,
    TableName: configuration["TableName"]!,
    DataMapper: new JsonReplicationDataMapper());

var storageConnectionString = configuration["StorageConnectionString"];

var client = new QueueClient(storageConnectionString, "outbox-queue");

await client.CreateIfNotExistsAsync(cancellationToken: cts.Token);

var subscription = new Subscription();
await foreach (var json in subscription.Subscribe(subscriptionOptions, cts.Token))
{
    await client.SendMessageAsync(json, cts.Token);
}