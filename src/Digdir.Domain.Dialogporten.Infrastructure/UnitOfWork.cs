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
using Polly.Timeout;

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
            return await SaveChangesOptimisticConcurrencyAsync(cancellationToken);
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

    private async Task<SaveChangesResult> SaveChangesOptimisticConcurrencyAsync(CancellationToken cancellationToken)
    {
        // Total timeout for optimistic concurrency handling
        const int timeoutInSeconds = 10;
        var timeoutPolicy = Policy.TimeoutAsync(timeoutInSeconds,
            TimeoutStrategy.Pessimistic,
            (_, _, _) => throw new OptimisticConcurrencyTimeoutException());

        // Backoff strategy with jitter for retry policy, starting at ~5ms
        const int medianFirstDelayInMs = 5;
        var jitterDelay = Backoff.DecorrelatedJitterBackoffV2(
            medianFirstRetryDelay: TimeSpan.FromMilliseconds(medianFirstDelayInMs),
            retryCount: int.MaxValue);

        // Fetch the db revision and retry
        // https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#resolving-concurrency-conflicts
        var retryPolicy = Policy
            .Handle<DbUpdateConcurrencyException>()
            .WaitAndRetryAsync(jitterDelay, (exception, _) =>
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

        var concurrencyRetryPolicy = timeoutPolicy.WrapAsync(retryPolicy);
        await concurrencyRetryPolicy.ExecuteAsync(async () =>
        {
            await _dialogDbContext.SaveChangesAsync(cancellationToken);
        });

        return new Success();
    }
}

internal class OptimisticConcurrencyTimeoutException : Exception;
