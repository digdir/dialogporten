using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.Common.Exceptions;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.Abstractions.Features.Versionable;
using Digdir.Library.Entity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Timeout;
using Polly.Wrap;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
    private static readonly AsyncPolicyWrap ConcurrencyRetryPolicy;

    private readonly DialogDbContext _dialogDbContext;
    private readonly ITransactionTime _transactionTime;
    private readonly IDomainContext _domainContext;

    private bool _auditableSideEffects = true;

    public UnitOfWork(DialogDbContext dialogDbContext, ITransactionTime transactionTime, IDomainContext domainContext)
    {
        _dialogDbContext = dialogDbContext ?? throw new ArgumentNullException(nameof(dialogDbContext));
        _transactionTime = transactionTime ?? throw new ArgumentNullException(nameof(transactionTime));
        _domainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
    }

    static UnitOfWork()
    {
        // Backoff strategy with jitter for retry policy, starting at ~5ms
        const int MedianFirstDelayInMs = 5;
        // Optimistic concurrency
        // Total timeout for optimistic concurrency handling
        const int TimeoutInSeconds = 10;

        var timeoutPolicy =
            Policy.TimeoutAsync(TimeoutInSeconds,
                TimeoutStrategy.Pessimistic,
                (_, _, _) => throw new OptimisticConcurrencyTimeoutException());

        // Fetch the db revision and retry
        // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#resolving-concurrency-conflicts
        var retryPolicy = Policy
            .Handle<DbUpdateConcurrencyException>()
            .WaitAndRetryAsync(
                sleepDurations: Backoff.DecorrelatedJitterBackoffV2(
                    medianFirstRetryDelay: TimeSpan.FromMilliseconds(MedianFirstDelayInMs),
                    retryCount: int.MaxValue),
                onRetryAsync: FetchCurrentRevision);

        ConcurrencyRetryPolicy = timeoutPolicy.WrapAsync(retryPolicy);
    }

    public IUnitOfWork WithoutAuditableSideEffects()
    {
        _auditableSideEffects = false;
        return this;
    }

    public async Task<SaveChangesResult> SaveChangesAsync(bool optimisticConcurrency, CancellationToken cancellationToken = default)
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

        if (optimisticConcurrency)
        {
            await ConcurrencyRetryPolicy.ExecuteAsync(ct => _dialogDbContext.SaveChangesAsync(ct), cancellationToken);

            return new Success();
        }

        try
        {
            await _dialogDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ConcurrencyError();
        }

        return new Success();
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
