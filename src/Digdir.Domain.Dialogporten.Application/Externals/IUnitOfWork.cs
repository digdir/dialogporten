using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using OneOf.Types;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IUnitOfWork
{
    IUnitOfWork WithoutAuditableSideEffects();
    Task<OneOf<Success, DomainError, UpdateConcurrencyError>> SaveChangesAsync(CancellationToken cancellationToken = default);
}