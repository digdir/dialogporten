using Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application;

public sealed class ApplicationSettings
{
    public const string ConfigurationSectionName = "Application";

    public required DialogportenSettings Dialogporten { get; init; }
}

public sealed class DialogportenSettings
{
    public required Uri BaseUri { get; init; }
}

internal sealed class ApplicationSettingsValidator : AbstractValidator<ApplicationSettings>
{
    public ApplicationSettingsValidator(IValidator<DialogportenSettings> dialogportenSettingsValidator)
    {
        RuleFor(x => x.Dialogporten)
            .NotEmpty()
            .SetValidator(dialogportenSettingsValidator);
    }
}

internal sealed class DialogportenSettingsValidator : AbstractValidator<DialogportenSettings>
{
    public DialogportenSettingsValidator()
    {
        RuleFor(x => x.BaseUri).NotEmpty().IsValidUri();
    }
}
