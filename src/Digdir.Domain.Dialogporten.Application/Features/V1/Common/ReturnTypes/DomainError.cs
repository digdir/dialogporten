using Digdir.Domain.Dialogporten.Domain.Common;

namespace Digdir.Domain.Dialogporten.Application.Features.V1.Common.ReturnTypes;

public record DomainError(IEnumerable<DomainFailure> Errors)
{
    public DomainError(DomainFailure error) : this(new[] { error }) { }
}
