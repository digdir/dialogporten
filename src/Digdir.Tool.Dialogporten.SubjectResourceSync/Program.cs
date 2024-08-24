using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(x => x.AddUserSecrets<Program>(optional: true, reloadOnChange: false))
    .ConfigureServices(services => {
        services.AddHttpClient<IResourceRegistry, ResourceRegistryClient>();
    })
    .Build();

host.Run();
