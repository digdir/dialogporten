using Digdir.Domain.Dialogporten.Application.Common.Numbers;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;

internal static class FluentValidation_Guid_Extensions
{
    private const string IsValidUuidV7Message = "'{PropertyName}' is not a valid v7 uuid.";

    public static IRuleBuilderOptions<T, Guid?> IsValidUuidV7<T>(this IRuleBuilder<T, Guid?> ruleBuilder)
    {
        return ruleBuilder
            .Must(IsValidUuidV7Predicate)
            .WithMessage(IsValidUuidV7Message);
    }

    public static IRuleBuilderOptions<T, Guid> IsValidUuidV7<T>(this IRuleBuilder<T, Guid> ruleBuilder)
    {
        return ruleBuilder
            .Must(x => IsValidUuidV7Predicate(x))
            .WithMessage(IsValidUuidV7Message);
    }

    private static bool IsValidUuidV7Predicate(this Guid? guid)
        => !guid.HasValue || UuidV7.IsValid(guid.Value);
}
