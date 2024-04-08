using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;

internal static class FluentValidationStringExtensions
{
    public static IRuleBuilderOptions<T, string?> IsValidUri<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(uri => uri is null || Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute))
            .WithMessage("'{PropertyName}' is not a well formatted URI.");
    }
}
