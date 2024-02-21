using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;

public record BadRequest(List<string> Reasons)
{
    private const string BadRequestMessage = "BadRequest";

    public BadRequest(params string[] reasons) : this(reasons.ToList()) { }

    public List<ValidationFailure> ToValidationResults() =>
        Reasons.Select(x => new ValidationFailure(BadRequestMessage, x)).ToList();
}
