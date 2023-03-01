using Digdir.Domain.Dialogporten.Application.Common.Behaviours;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Digdir.Domain.Dialogporten.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, Action<ApplicationSettings>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        if (configure is not null)
        {
            services.Configure(configure);
        }

        var thisAssembly = Assembly.GetExecutingAssembly();

        return services

            // Framework
            .AddAutoMapper(thisAssembly)
            .AddMediatR(x => x.RegisterServicesFromAssembly(thisAssembly))
            .AddValidatorsFromAssembly(thisAssembly, includeInternalTypes: true)

            // Transient
            .AddTransient<ILocalizationService, LocalizationService>()
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
    }
}
