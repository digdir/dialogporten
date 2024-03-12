using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;

public record Forbidden(List<string> Reasons)
{
    private const string ForbiddenMessage = "Forbidden";

    public Forbidden(params string[] reasons) : this(reasons.ToList()) { }

    public List<ValidationFailure> ToValidationResults() =>
        [.. Reasons.Select(x => new ValidationFailure(ForbiddenMessage, x))];

    public Forbidden WithMissingScopes(params string[] scopes)
    {
        Reasons.Add($"Missing scopes: ({string.Join(", ", scopes)}).");
        return this;
    }
}
