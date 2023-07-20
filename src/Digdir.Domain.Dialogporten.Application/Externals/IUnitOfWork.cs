using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using OneOf.Types;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IUnitOfWork
{
    IUnitOfWork WithoutAuditableSideEffects();
    Task<SaveChangesResult> SaveChangesAsync(CancellationToken cancellationToken = default);
}

[GenerateOneOf]
public partial class SaveChangesResult : OneOfBase<Success, DomainError, ConcurrencyError> { }
