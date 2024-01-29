using Digdir.Domain.Dialogporten.Domain.Parties;
using Digdir.Domain.Dialogporten.Domain.Parties.Abstractions;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.Application.Common.Extensions.FluentValidation;

public static class FluentValidationPartyIdentifierExtensions
{
    public static IRuleBuilderOptions<T, string> IsValidPartyIdentifier<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Must(identifier => identifier is null || NorwegianPersonIdentifier.IsValid(identifier) || NorwegianOrganizationIdentifier.IsValid(identifier))
            .WithMessage(
                $"'{{PropertyName}}' must be on format '{NorwegianOrganizationIdentifier.Prefix}{{norwegian org-nr}}' or " +
                $"'{NorwegianPersonIdentifier.Prefix}{{{{norwegian f-nr/d-nr}}}}' with valid numbers respectively.");
    }
}
