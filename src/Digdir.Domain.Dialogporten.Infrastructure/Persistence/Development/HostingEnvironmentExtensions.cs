using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace Digdir.Domain.Dialogporten.Infrastructure.Persistence.Development;

internal static class HostingEnvironmentExtensions
{
    public static bool ShouldRunDevelopmentHostedService(this IHostEnvironment environment)
    {
        // Only run in development environments, but not when using Janitor
        return environment.IsDevelopment() && !(Assembly.GetEntryAssembly()?.GetName().Name ?? "").Contains("Janitor");
    }
}
