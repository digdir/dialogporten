using System.Diagnostics;
using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
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
            await ConcurrencyRetryPolicy.ExecuteAsync(async () =>
            {
                await _dialogDbContext.SaveChangesAsync(cancellationToken);
            });

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

    // Optimistic concurrency
    // Total timeout for optimistic concurrency handling
    private const int TimeoutInSeconds = 10;
    private static readonly AsyncTimeoutPolicy TimeoutPolicy =
        Policy.TimeoutAsync(TimeoutInSeconds,
        TimeoutStrategy.Pessimistic,
        (_, _, _) => throw new OptimisticConcurrencyTimeoutException());

    // Backoff strategy with jitter for retry policy, starting at ~5ms
    private const int MedianFirstDelayInMs = 5;
    private static readonly IEnumerable<TimeSpan> JitterDelay = Backoff.DecorrelatedJitterBackoffV2(
        medianFirstRetryDelay: TimeSpan.FromMilliseconds(MedianFirstDelayInMs),
        retryCount: int.MaxValue);

    // Fetch the db revision and retry
    // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#resolving-concurrency-conflicts
    private static readonly AsyncRetryPolicy RetryPolicy = Policy
        .Handle<DbUpdateConcurrencyException>()
        .WaitAndRetryAsync(JitterDelay, (exception, _) =>
        {
            if (exception is not DbUpdateConcurrencyException concurrencyException)
            {
                return;
            }

            foreach (var entry in concurrencyException.Entries)
            {
                var dbValues = entry.GetDatabaseValues();
                var revision = dbValues![nameof(DialogEntity.Revision)];
                entry.CurrentValues[nameof(DialogEntity.Revision)] = revision;
                entry.OriginalValues.SetValues(dbValues);
            }
        });

    private static readonly AsyncPolicyWrap ConcurrencyRetryPolicy = TimeoutPolicy.WrapAsync(RetryPolicy);
}

internal class OptimisticConcurrencyTimeoutException : Exception;
