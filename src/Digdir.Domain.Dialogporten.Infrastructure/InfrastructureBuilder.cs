using System.Reflection;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.OptionExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using static Digdir.Domain.Dialogporten.Infrastructure.InfrastructureExtensions;

namespace Digdir.Domain.Dialogporten.Infrastructure;

public interface IInfrastructureBuilder
{
    IServiceCollection Build();
}

public interface IPubSubInfrastructureChoice
{
    IInfrastructureBuilder WithPubSubCapabilities<TConsumerAssembly>() => WithPubSubCapabilities(typeof(TConsumerAssembly).Assembly);
    IInfrastructureBuilder WithPubSubCapabilities(params Assembly[] consumerAssemblies);
    IInfrastructureBuilder WithoutPubSubCapabilities();
    IInfrastructureBuilder WithPubCapabilities();
}

internal sealed class InfrastructureBuilder(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment) :
    IInfrastructureBuilder,
    IPubSubInfrastructureChoice
{
    private readonly IServiceCollection _services = services ?? throw new ArgumentNullException(nameof(services));
    private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    private readonly IHostEnvironment _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    private readonly List<Action<InfrastructureBuilderContext>> _actions = [];

    public IServiceCollection Build()
    {
        var settings = RegisterAndValidateInfrastructureSettings();
        var builderContext = new InfrastructureBuilderContext(_services, _configuration, _environment, settings);
        AddInfrastructure_Internal(builderContext);
        foreach (var action in _actions)
        {
            action(builderContext);
        }
        return _services;
    }

    public IInfrastructureBuilder WithPubSubCapabilities(params Assembly[] consumerAssemblies)
        => AddAction(context => AddPubSubCapabilities(context, consumerAssemblies));
    public IInfrastructureBuilder WithPubCapabilities() => AddAction(AddPubCapabilities);
    public IInfrastructureBuilder WithoutPubSubCapabilities() => this;

    private InfrastructureBuilder AddAction(Action<InfrastructureBuilderContext> action)
    {
        _actions.Add(action);
        return this;
    }

    private InfrastructureSettings RegisterAndValidateInfrastructureSettings()
    {
        var infrastructureConfigurationSection = configuration
            .GetSection(InfrastructureSettings.ConfigurationSectionName);
        services.AddOptions<InfrastructureSettings>()
            .Bind(infrastructureConfigurationSection)
            .ValidateFluently();
        var settings = infrastructureConfigurationSection.Get<InfrastructureSettings>()
                       ?? throw new InvalidOperationException("Infrastructure settings are not configured.");
        var validator = new FluentValidationOptions<InfrastructureSettings>(InfrastructureAssemblyMarker.Assembly);
        var validatorResult = validator.Validate(null, settings);
        return validatorResult.Failed
            ? throw new InvalidOperationException($"Infrastructure settings are not valid.{Environment.NewLine}{validatorResult.FailureMessage}")
            : settings;
    }
}

internal sealed record InfrastructureBuilderContext(
    IServiceCollection Services,
    IConfiguration Configuration,
    IHostEnvironment Environment,
    InfrastructureSettings Settings);
