using System.Diagnostics;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.OptionExtensions;

public sealed class FluentValidationOptions<TOptions> : IValidateOptions<TOptions>
    where TOptions : class
{
    private static readonly Type OptionType = typeof(TOptions);
    private readonly IEnumerable<IValidator<TOptions>> _validators;
    public string? Name { get; }

    public FluentValidationOptions(string? name, IEnumerable<IValidator<TOptions>> validators)
    {
        Name = name;
        _validators = validators;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentValidationOptions{TOptions}"/> class
    /// by scanning the specified assemblies for validators.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for validators.</param>
    /// <remarks>
    /// This constructor scans the provided assemblies for types that implement the 
    /// <see cref="IValidator{T}"/> interface for the specified <typeparamref name="TOptions"/> type.
    /// It includes both public and internal validators in the search. <b>Use this constructor sparingly
    /// as it uses reflection to find validators.</b>
    /// </remarks>
    public FluentValidationOptions(params Assembly[] assemblies)
    {
        _validators = AssemblyScanner
            .FindValidatorsInAssemblies(assemblies, includeInternalTypes: true)
            .Where(x => x.InterfaceType.GenericTypeArguments.First() == OptionType)
            .Select(x => (IValidator<TOptions>)Activator.CreateInstance(x.ValidatorType, nonPublic: true)! ?? throw new UnreachableException())
            .ToList();
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
