using Digdir.Domain.Dialogporten.Domain.Common;
using FluentValidation;
using FluentValidation.Validators;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common;

public static class UuiDv7ValidatorExtensions
{
    public static IRuleBuilderOptions<T, Guid?> IsValidUuidV7<T>(this IRuleBuilder<T, Guid?> ruleBuilder)
        => ruleBuilder.SetValidator(new IsValidUuidV7Validator<T>());

    public static void UuidV7TimestampIsInPast<T>(this IRuleBuilder<T, Guid?> ruleBuilder)
        => ruleBuilder.SetValidator(new IsValidUuidV7TimestampValidator<T>());
}

public sealed class IsValidUuidV7Validator<T> : PropertyValidator<T, Guid?>
{
    public override bool IsValid(ValidationContext<T> context, Guid? value)
        => value is null || UuidV7.IsValid(value.Value);

    public override string Name { get; } = "Uuid7Validator";

    protected override string GetDefaultMessageTemplate(string errorCode) =>
        "Invalid id: {PropertyValue}. Must be in big endian UUIDv7 format, see [link TBD].";
}

public sealed class IsValidUuidV7TimestampValidator<T> : PropertyValidator<T, Guid?>
{
    public override bool IsValid(ValidationContext<T> context, Guid? value)
    {
        if (value is null)
        {
            return true;
        }

        if (!UuidV7.TryToDateTimeOffset(value.Value, out var date))
        {
            context.MessageFormatter.AppendArgument("date", "invalid date");
            return false;
        }

        context.MessageFormatter.AppendArgument("date", date.ToString("o"));
        return date < DateTimeOffset.UtcNow;
    }

    public override string Name { get; } = "Uuid7TimestampValidator";

    protected override string GetDefaultMessageTemplate(string errorCode)
        => "Invalid {PropertyName}. Expected the unix timestamp portion of the UUIDv7 to be in the past. Extrapolated '{date}' from '{PropertyValue}'. See [link TDB]";
}
