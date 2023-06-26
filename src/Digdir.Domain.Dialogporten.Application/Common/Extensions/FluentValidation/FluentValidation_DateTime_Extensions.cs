using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;

internal static class FluentValidation_DateTime_Extensions
{
    public const string IsUtcKindMessage =
        "'{PropertyName}' must be an ISO 8601 formated UTC time. For example " +
        "{PropertyValue:yyyy-MM-ddTHH:mm:ss.fffffff}Z.";

    public const string InPastMessage = "'{PropertyName}' must be in the past.";
    public const string InFutureMessage = "'{PropertyName}' must be in the future.";

    public const string InPastOfMessage = "'{PropertyName}' must be before '{ComparisonProperty}'.";
    public const string InFutureOfMessage = "'{PropertyName}' must be after '{ComparisonProperty}'.";

    public static IRuleBuilderOptions<T, DateTime?> IsInFuture<T>(this IRuleBuilder<T, DateTime?> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage(InFutureMessage);
    }

    public static IRuleBuilderOptions<T, DateTime> IsInFuture<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(DateTime.UtcNow)
            .WithMessage(InFutureMessage);
    }

    public static IRuleBuilderOptions<T, DateTime?> IsInPast<T>(this IRuleBuilder<T, DateTime?> ruleBuilder)
    {
        return ruleBuilder
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage(InPastMessage);
    }

    public static IRuleBuilderOptions<T, DateTime> IsInPast<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage(InPastMessage);
    }

    public static IRuleBuilderOptions<T, DateTime?> IsUtcKind<T>(this IRuleBuilder<T, DateTime?> ruleBuilder)
    {
        return ruleBuilder
            .Must(IsUtcKindPredicate)
            .WithMessage(IsUtcKindMessage);
    }

    public static IRuleBuilderOptions<T, DateTime> IsUtcKind<T>(this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder
            .Must(x => IsUtcKindPredicate(x))
            .WithMessage(IsUtcKindMessage);
    }

    private static bool IsUtcKindPredicate(this DateTime? dateTime)
        => !dateTime.HasValue || dateTime.Value.Kind == DateTimeKind.Utc;
}