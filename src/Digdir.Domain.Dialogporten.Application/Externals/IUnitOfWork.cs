using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Library.Entity.Abstractions.Features.Versionable;
using OneOf.Types;
using OneOf;

namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IUnitOfWork
{
    IUnitOfWork WithoutAggregateSideEffects();
    Task<SaveChangesResult> SaveChangesAsync(CancellationToken cancellationToken = default);

    IUnitOfWork EnableConcurrencyCheck<TEntity>(
        TEntity? entity,
        Guid? revision)
        where TEntity : class, IVersionableEntity;

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
}

[GenerateOneOf]
public sealed partial class SaveChangesResult : OneOfBase<Foo, DomainError, ConcurrencyError>;

#pragma warning disable CA1707
public sealed class Foo
#pragma warning restore CA1707
{
    public Guid NewDialogRevision { get; set; }
}
