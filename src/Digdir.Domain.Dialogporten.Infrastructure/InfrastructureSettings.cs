using Altinn.ApiClients.Maskinporten.Config;
using Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Infrastructure;

public sealed class InfrastructureSettings
{
    public const string ConfigurationSectionName = "Infrastructure";

    public required string DialogDbConnectionString { get; init; }
    public required AltinnPlatformSettings Altinn { get; init; }
    public required MaskinportenSettings Maskinporten { get; init; }
}

public sealed class AltinnPlatformSettings
{
    public required Uri BaseUri { get; init; }
    public required string SubscriptionKey { get; init; }
}

internal sealed class InfrastructureSettingsValidator : AbstractValidator<InfrastructureSettings>
{
    public InfrastructureSettingsValidator(
        IValidator<AltinnPlatformSettings> altinnPlatformSettingsValidator,
        IValidator<MaskinportenSettings> maskinportenSettingsValidator)
    {
        RuleFor(x => x.DialogDbConnectionString)
            .NotEmpty();

        RuleFor(x => x.Altinn)
            .NotEmpty()
            .SetValidator(altinnPlatformSettingsValidator);

        RuleFor(x => x.Maskinporten)
            .NotEmpty()
            .SetValidator(maskinportenSettingsValidator);
    }
}

internal sealed class AltinnPlatformSettingsValidator : AbstractValidator<AltinnPlatformSettings>
{
    public AltinnPlatformSettingsValidator()
    {
        RuleFor(x => x.BaseUri).NotEmpty().IsValidUri();
    }
}

internal sealed class MaskinportenSettingsValidator : AbstractValidator<MaskinportenSettings>
{
    public MaskinportenSettingsValidator()
    {
        RuleFor(x => x.ClientId).NotEmpty();
        RuleFor(x => x.Scope).NotEmpty();
        RuleFor(x => x.Environment).NotEmpty();
        RuleFor(x => x.EncodedJwk).NotEmpty();
    }
}
