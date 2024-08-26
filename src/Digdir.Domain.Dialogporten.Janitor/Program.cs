using Digdir.Domain.Dialogporten.Application;
using Digdir.Domain.Dialogporten.Infrastructure;
using Digdir.Domain.Dialogporten.Janitor.Features.UpdateSubjectResources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

await Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, configurationBuilder) =>
    {
        var env = context.HostingEnvironment;

        configurationBuilder.SetBasePath(env.ContentRootPath);
        configurationBuilder.AddJsonFile("appsettings.json", optional: false);
        configurationBuilder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
        configurationBuilder.AddEnvironmentVariables();
        configurationBuilder.AddUserSecrets<Program>(optional: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddApplication(context.Configuration, context.HostingEnvironment);
        services.AddInfrastructure(context.Configuration, context.HostingEnvironment);

        services.AddTransient<UpdateSubjectResources>();
    })
    .ConfigureCocona(args, new[] { typeof(Program) })
    .Build()
    .RunAsync();

#pragma warning disable CS8321 // Local function is declared but never used
static async Task UpdateSubjectResources(UpdateSubjectResources updateSubjectResources)
{
    await updateSubjectResources.RunAsync(default);
}


#pragma warning restore CS8321 // Local function is declared but never used
