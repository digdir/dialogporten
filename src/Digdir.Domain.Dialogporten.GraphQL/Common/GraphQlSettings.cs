using Digdir.Domain.Dialogporten.GraphQL.Common.Authentication;
using FluentValidation;

namespace Digdir.Domain.Dialogporten.GraphQL.Common;

public sealed class GraphQlSettings
{
    public const string SectionName = "GraphQl";

    public required AuthenticationOptions Authentication { get; init; }
}

internal sealed class GraphQlOptionsValidator : AbstractValidator<GraphQlSettings>
{
    public GraphQlOptionsValidator(
               IValidator<AuthenticationOptions> authenticationOptionsValidator)
    {
        RuleFor(x => x.Authentication)
            .SetValidator(authenticationOptionsValidator);
    }
}
