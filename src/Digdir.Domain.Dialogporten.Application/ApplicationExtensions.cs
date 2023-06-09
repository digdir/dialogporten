using Digdir.Domain.Dialogporten.Application.Common.Behaviours;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common;
using Digdir.Domain.Dialogporten.Application.Features.V1.Dialogs.Queries.Get;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

namespace Digdir.Domain.Dialogporten.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configurationSection)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configurationSection);
        var thisAssembly = Assembly.GetExecutingAssembly();
        return services
            // Settings
            .Configure<ApplicationSettings>(configurationSection)

            // Framework
            .AddAutoMapper(thisAssembly)
            .AddMediatR(x => x.RegisterServicesFromAssembly(thisAssembly))
            .AddValidatorsFromAssembly(thisAssembly, includeInternalTypes: true)

            // Transient
            .AddTransient<ILocalizationService, LocalizationService>()
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
    }
}
