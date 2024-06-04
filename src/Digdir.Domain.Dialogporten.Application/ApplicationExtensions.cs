using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.Behaviours;
using Digdir.Domain.Dialogporten.Application.Common.Extensions;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.OptionExtensions;
using Digdir.Domain.Dialogporten.Application.Common.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Digdir.Domain.Dialogporten.Application;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        var thisAssembly = Assembly.GetExecutingAssembly();

        services.AddOptions<ApplicationSettings>()
            .Bind(configuration.GetSection(ApplicationSettings.ConfigurationSectionName))
            .ValidateFluently()
            .ValidateOnStart();

        services
            // Framework
            .AddAutoMapper(thisAssembly)
            .AddMediatR(x => x.RegisterServicesFromAssembly(thisAssembly))
            .AddValidatorsFromAssembly(thisAssembly, ServiceLifetime.Transient, includeInternalTypes: true)

            // Singleton
            .AddSingleton<ICompactJwsGenerator, Ed25519Generator>()

            // Scoped
            .AddScoped<IDomainContext, DomainContext>()
            .AddScoped<ITransactionTime, TransactionTime>()
            .AddScoped<IDialogTokenGenerator, DialogTokenGenerator>()

            // Transient
            .AddTransient<IStringHasher, PersistentRandomSaltStringHasher>()
            .AddTransient<IUserOrganizationRegistry, UserOrganizationRegistry>()
            .AddTransient<IUserResourceRegistry, UserResourceRegistry>()
            .AddTransient<IUserNameRegistry, UserNameRegistry>()
            .AddTransient<IUserParties, UserParties>()
            .AddTransient<IDialogActivityService, DialogActivityService>()
            .AddTransient<IClock, Clock>()
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>))
            .AddTransient(typeof(IPipelineBehavior<,>), typeof(DomainContextBehaviour<,>));

        if (!environment.IsDevelopment())
        {
            return services;
        }

        var localDeveloperSettings = configuration.GetLocalDevelopmentSettings();
        services.Decorate<IUserResourceRegistry, LocalDevelopmentUserResourceRegistryDecorator>(
            predicate:
            localDeveloperSettings.UseLocalDevelopmentUser ||
            localDeveloperSettings.UseLocalDevelopmentResourceRegister);

        services.Decorate<IUserOrganizationRegistry, LocalDevelopmentUserOrganizationRegistryDecorator>(
            predicate:
            localDeveloperSettings.UseLocalDevelopmentUser ||
            localDeveloperSettings.UseLocalDevelopmentOrganizationRegister);

        services.Decorate<IUserNameRegistry, LocalDevelopmentUserNameRegistryDecorator>(
            predicate:
            localDeveloperSettings.UseLocalDevelopmentUser ||
            localDeveloperSettings.UseLocalDevelopmentNameRegister);

        services.Decorate<ICompactJwsGenerator, LocalDevelopmentCompactJwsGeneratorDecorator>(
            predicate: localDeveloperSettings.UseLocalDevelopmentCompactJwsGenerator);

        return services;
    }
}
