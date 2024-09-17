using Digdir.Domain.Dialogporten.Domain.Common;
using FluentValidation.Results;

namespace Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;

public sealed record DomainError(IEnumerable<DomainFailure> Errors)
{
    public DomainError(DomainFailure error) : this([error]) { }
    public List<ValidationFailure> ToValidationResults() => Errors
        .Select(x => new ValidationFailure(x.PropertyName, x.ErrorMessage))
        .ToList();
}
