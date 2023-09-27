using FluentValidation;
using Microsoft.Extensions.Options;

namespace Digdir.Domain.Dialogporten.WebApi.Common.OptionExtensions;

internal static class OptionsBuilderFluentValidationExtensions
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
