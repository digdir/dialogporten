using Digdir.Domain.Dialogporten.Domain.Common;
using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;

public record DomainError(IEnumerable<DomainFailure> Errors)
{
    public DomainError(DomainFailure error) : this(new[] { error }) { }
    public List<ValidationFailure> ToValidationResults() => Errors
        .Select(x => new ValidationFailure(x.PropertyName, x.ErrorMessage))
        .ToList();
}
