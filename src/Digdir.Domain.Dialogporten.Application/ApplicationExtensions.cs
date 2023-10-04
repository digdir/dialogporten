using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Behaviours;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.OptionExtensions;
using Digdir.Domain.Dialogporten.Application.Features.V1.Common.Localizations;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Digdir.Domain.Dialogporten.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        var thisAssembly = Assembly.GetExecutingAssembly();

        services.AddOptions<ApplicationSettings>()
            .Bind(configuration.GetSection(ApplicationSettings.ConfigurationSectionName))
            .ValidateFluently()
            .ValidateOnStart();

        return services
            // Framework
            .AddAutoMapper(thisAssembly)
            .AddMediatR(x => x.RegisterServicesFromAssembly(thisAssembly))
            .AddValidatorsFromAssembly(thisAssembly, ServiceLifetime.Transient, includeInternalTypes: true)

            // Scoped
            .AddScoped<IDomainContext, DomainContext>()
            .AddScoped<ITransactionTime, TransactionTime>()

            // Transient
            .AddTransient<UserService>()
            .AddTransient<ILocalizationService, LocalizationService>()
            .AddTransient<IClock, Clock>()
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainContextBehaviour<,>));
    }
}
