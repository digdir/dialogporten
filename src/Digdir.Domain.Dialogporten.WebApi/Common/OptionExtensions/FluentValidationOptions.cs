using FluentValidation;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.WebApi.Common.OptionExtensions;

public class FluentValidationOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    private readonly IEnumerable<IValidator<TOptions>> _validators;
    public string? Name { get; }

    public FluentValidationOptions(string? name, IEnumerable<IValidator<TOptions>> validators)
    {
        Name = name;
        _validators = validators;
    }

    public ValidateOptionsResult Validate(string? name, TOptions options)
    {
        // Null name is used to configure all named options.
        if (Name is not null && Name != name)
        {
            // Ignored if not validation this instance.
            return ValidateOptionsResult.Skip;
        }

        ArgumentNullException.ThrowIfNull(options);

        var failures = _validators
            .Select(v => v.Validate(options))
            .SelectMany(x => x.Errors)
            .Where(x => x is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return ValidateOptionsResult.Success;
        }

        var errors = failures
            .Select(x => $"{x.PropertyName}: {x.ErrorMessage}");
        return ValidateOptionsResult.Fail(errors);
    }
}