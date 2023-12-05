using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;

public record Forbidden(IEnumerable<object> Scopes)
{
    private string Message => $"Missing scopes: ({string.Join(", ", Scopes)}). ";

    public List<ValidationFailure> ToValidationResults() => new() { new ValidationFailure("Forbidden", Message) };
}
