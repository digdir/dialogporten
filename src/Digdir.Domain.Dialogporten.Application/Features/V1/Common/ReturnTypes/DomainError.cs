using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;

public record DomainError(IEnumerable<ValidationFailure> Errors)
{
    public DomainError(ValidationFailure error) : this(new[] { error }) { }
}
