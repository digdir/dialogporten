using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;

public sealed record Conflict(string Message)
{
    private const string ConflictMessage = "Conflict";
    public List<ValidationFailure> ToValidationResults() => [new(ConflictMessage, Message)];
}
