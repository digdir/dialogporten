using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.OptionExtensions;

public static class OptionsBuilderFluentValidationExtensions
{
    public static OptionsBuilder<TOptions> ValidateFluently<TOptions>(this OptionsBuilder<TOptions> optionsBuilder)
        where TOptions : class
    {
        optionsBuilder.Services.AddTransient<IValidateOptions<TOptions>>(x =>
            new FluentValidationOptions<TOptions>(
                optionsBuilder.Name,
                x.GetRequiredService<IEnumerable<IValidator<TOptions>>>()));
        return optionsBuilder;
    }
}
