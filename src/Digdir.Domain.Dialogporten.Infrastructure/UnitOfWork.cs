using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.Abstractions.Features.Versionable;
using Digdir.Library.Entity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
    // Fetch the db revision and retry
    // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#resolving-concurrency-conflicts
    private static readonly AsyncPolicy ConcurrencyRetryPolicy = Policy
        .Handle<DbUpdateConcurrencyException>()
        .WaitAndRetryAsync(
            sleepDurations: Backoff.ConstantBackoff(TimeSpan.FromMilliseconds(200), 25),
            onRetryAsync: FetchCurrentRevision);

    private readonly DialogDbContext _dialogDbContext;
    private readonly ITransactionTime _transactionTime;
    private readonly IDomainContext _domainContext;

    private bool _auditableSideEffects = true;
    private bool _enableConcurrencyCheck;

    public UnitOfWork(DialogDbContext dialogDbContext, ITransactionTime transactionTime, IDomainContext domainContext)
    {
        _dialogDbContext = dialogDbContext ?? throw new ArgumentNullException(nameof(dialogDbContext));
        _transactionTime = transactionTime ?? throw new ArgumentNullException(nameof(transactionTime));
        _domainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
    }

    public IUnitOfWork EnableConcurrencyCheck<TEntity>(
        TEntity? entity,
        Guid? revision)
        where TEntity : class, IVersionableEntity
    {
        if (_dialogDbContext.TrySetOriginalRevision(entity, revision))
        {
            _enableConcurrencyCheck = true;
        }

        return this;
    }

    public IUnitOfWork WithoutAuditableSideEffects()
    {
        _auditableSideEffects = false;
        return this;
    }

    public async Task<SaveChangesResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!_domainContext.IsValid)
        {
            return new DomainError(_domainContext.Pop());
        }

        if (!_dialogDbContext.ChangeTracker.HasChanges())
        {
            return new Success();
        }

        if (_auditableSideEffects)
        {
            await _dialogDbContext.ChangeTracker.HandleAuditableEntities(_transactionTime.Value, cancellationToken);
        }

        if (!_enableConcurrencyCheck)
        {
            // Attempt to save changes without concurrency check
            await ConcurrencyRetryPolicy.ExecuteAsync(_dialogDbContext.SaveChangesAsync, cancellationToken);
        }
        else
        {
            try
            {
                await _dialogDbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ConcurrencyError();
            }
        }

        // Interceptors can add domain errors, so check again
        return !_domainContext.IsValid
            ? new DomainError(_domainContext.Pop())
            : new Success();
    }

    private static async Task FetchCurrentRevision(Exception exception, TimeSpan _)
    {
        if (exception is not DbUpdateConcurrencyException concurrencyException)
        {
            return;
        }

        foreach (var entry in concurrencyException.Entries)
        {
            if (entry.Entity is not IVersionableEntity)
            {
                continue;
            }

            var dbValues = await entry.GetDatabaseValuesAsync();
            if (dbValues == null)
            {
                continue;
            }

            var currentRevision = dbValues[nameof(IVersionableEntity.Revision)]!;
            entry.Property(nameof(IVersionableEntity.Revision)).OriginalValue = currentRevision;
        }
    }
}
