using Digdir.Domain.Dialogporten.Application.Common.Interfaces;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Digdir.Domain.Dialogporten.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, Action<InfrastructureSettings>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        if (configure is not null)
        {
            services.Configure(configure);
        }

        return services

            // Framework
            .AddDbContext<DialogueDbContext>((services, options) => options
                .UseNpgsql("Server=localhost;Port=5432;Database=mydb;User ID=course;Password=changeme;"))
            //.UseSqlServer(services
            //    .GetRequiredService<IOptions<InfrastructureSettings>>()
            //    .Value.DialogueDbConnectionString))
            .AddHostedService<DevelopmentMigratorHostedService>()

            // Scoped
            .AddScoped<IDialogueDbContext>(x => x.GetRequiredService<DialogueDbContext>())
            .AddScoped<IUnitOfWork, UnitOfWork>();
    }
}
