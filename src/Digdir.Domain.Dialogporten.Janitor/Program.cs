using Cocona;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Janitor;
using Digdir.Domain.Dialogporten.Janitor.Features.UpdateSubjectResources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var cts = new CancellationTokenSource();
await Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((context, configurationBuilder) =>
    {
        var environmentName = context.HostingEnvironment.EnvironmentName.Replace("Production", "prod");
        Console.WriteLine($"Running in environment: {environmentName}");

        configurationBuilder.AddJsonFile("appsettings.json", optional: false);
        configurationBuilder.AddJsonFile($"appsettings.{environmentName}.json", optional: true);
        configurationBuilder.AddEnvironmentVariables();
        configurationBuilder.AddUserSecrets<Program>(optional: true);
    })
    .ConfigureServices((context, services) =>
    {
        services
            .AddApplication(context.Configuration, context.HostingEnvironment)
            .AddInfrastructure(context.Configuration, context.HostingEnvironment)
            .AddScoped<IUser, ConsoleUser>()
            .AddTransient<UpdateSubjectResources>(sp =>
            {
                var scope = sp.CreateScope();
                return new UpdateSubjectResources(
                    scope.ServiceProvider.GetRequiredService<ILogger<UpdateSubjectResources>>(),
                    scope.ServiceProvider.GetRequiredService<IResourceRegistry>(),
                    scope.ServiceProvider.GetRequiredService<IDialogDbContext>(),
                    scope.ServiceProvider.GetRequiredService<IUnitOfWork>()
                );
            });
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.AddConsole();
        logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Critical);
        logging.AddFilter("System.Net.Http.HttpClient.IResourceRegistry.ClientHandler", LogLevel.Warning);
    })
    .ConfigureCocona(args, [typeof(Commands)])
    .Build()
    .RunAsync(cts.Token);


#pragma warning disable CA1822 // Disable member can be static inspection (breaks Cocona)
internal sealed class Commands
{
    public async Task UpdateSubjectResources([FromService] UpdateSubjectResources updateSubjectResources, DateTimeOffset? since = null, CancellationToken cancellationToken = default)
    {
        await updateSubjectResources.RunAsync(since, cancellationToken);
    }

    public void Hello()
    {
        Console.WriteLine("Hello, World!");
    }
}
#pragma warning restore CA1822
