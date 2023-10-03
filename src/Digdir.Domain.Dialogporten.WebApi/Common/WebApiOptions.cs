using Digdir.Domain.Dialogporten.WebApi.Common.Options;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.WebApi.Common;

public sealed class WebApiOptions
{
    public const string SectionName = "WebApi";

    public required AuthenticationOptions Authentication { get; init; }
}

internal sealed class WebApiOptionsValidator : AbstractValidator<WebApiOptions>
{
    public WebApiOptionsValidator(
               IValidator<AuthenticationOptions> authenticationOptionsValidator)
    {
        RuleFor(x => x.Authentication)
            .SetValidator(authenticationOptionsValidator);
    }
}
