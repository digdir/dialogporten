using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Common;

public record ValidationFailed(IEnumerable<ValidationFailure> Errors)
{
    public ValidationFailed(ValidationFailure error) : this(new[] { error }) { }
}