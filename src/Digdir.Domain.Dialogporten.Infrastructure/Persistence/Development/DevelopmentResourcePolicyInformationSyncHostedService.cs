using Digdir.Domain.Dialogporten.Application.Features.V1.ResourceRegistry.Commands.Synchronize;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Development;

internal sealed class DevelopmentResourcePolicyInformationSyncHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public DevelopmentResourcePolicyInformationSyncHostedService(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_environment.ShouldRunDevelopmentHostedService())
        {
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        await sender.Send(new SynchronizeResourcePolicyInformationCommand(), cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}