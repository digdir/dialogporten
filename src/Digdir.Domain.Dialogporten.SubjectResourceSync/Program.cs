using Digdir.Domain.Dialogporten.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

await new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration((context, configurationBuilder) =>
    {
        var env = context.HostingEnvironment;

        configurationBuilder.SetBasePath(env.ContentRootPath);
        configurationBuilder.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        configurationBuilder.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
        configurationBuilder.AddEnvironmentVariables();
        configurationBuilder.AddUserSecrets<Program>(optional: true, reloadOnChange: false);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddInfrastructure(context.Configuration, context.HostingEnvironment);
    })
    .StartAsync();
