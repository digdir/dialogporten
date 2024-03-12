using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Library.Entity.Abstractions.Features.Versionable;
using OneOf.Types;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IUnitOfWork
{
    IUnitOfWork WithoutAuditableSideEffects();
    Task<SaveChangesResult> SaveChangesAsync(CancellationToken cancellationToken = default);

    IUnitOfWork EnableConcurrencyCheck<TEntity>(
        TEntity? entity,
        Guid? revision)
        where TEntity : class, IVersionableEntity;
}

[GenerateOneOf]
public partial class SaveChangesResult : OneOfBase<Success, DomainError, ConcurrencyError>;
