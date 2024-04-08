using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;

internal static class FluentValidationDateTimeOffsetExtensions
{
    public const string InPastMessage = "'{PropertyName}' must be in the past.";
    public const string InFutureMessage = "'{PropertyName}' must be in the future.";

    public const string InPastOfMessage = "'{PropertyName}' must be before '{ComparisonProperty}'.";
    public const string InFutureOfMessage = "'{PropertyName}' must be after '{ComparisonProperty}'.";

    public static IRuleBuilderOptions<T, DateTimeOffset?> IsInFuture<T>(this IRuleBuilder<T, DateTimeOffset?> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage(InFutureMessage);
    }

    public static IRuleBuilderOptions<T, DateTimeOffset> IsInFuture<T>(this IRuleBuilder<T, DateTimeOffset> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage(InFutureMessage);
    }

    public static IRuleBuilderOptions<T, DateTimeOffset?> IsInPast<T>(this IRuleBuilder<T, DateTimeOffset?> ruleBuilder)
    {
        return ruleBuilder
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage(InPastMessage);
    }

    public static IRuleBuilderOptions<T, DateTimeOffset> IsInPast<T>(this IRuleBuilder<T, DateTimeOffset> ruleBuilder)
    {
        return ruleBuilder
            .LessThanOrEqualTo(DateTimeOffset.UtcNow)
            .WithMessage(InPastMessage);
    }
}
