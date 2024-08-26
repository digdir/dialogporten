using Cocona;
using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Application.Externals.Presentation;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Janitor;
using Digdir.Domain.Dialogporten.Janitor.Features.UpdateSubjectResources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = CoconaApp.CreateHostBuilder()
    .ConfigureAppConfiguration((context, configurationBuilder) =>
    {
        var env = context.HostingEnvironment;

        configurationBuilder.AddJsonFile("appsettings.json", optional: false);
        configurationBuilder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
        configurationBuilder.AddEnvironmentVariables();
        configurationBuilder.AddUserSecrets<Program>(optional: true);
    })
    .ConfigureServices((context, services) =>
    {
        services
            .AddApplication(context.Configuration, context.HostingEnvironment)
            .AddInfrastructure(context.Configuration, context.HostingEnvironment)
            .AddScoped<IUser, ConsoleUser>()
            .AddTransient<UpdateSubjectResources>();
    });

await builder.RunAsync<Commands>(args);

#pragma warning disable CA1822
internal sealed class Commands
{
    public async Task UpdateSubjectResources([FromService] UpdateSubjectResources updateSubjectResources)
    {
        await updateSubjectResources.RunAsync(default);
    }
}
#pragma warning restore CA1822
