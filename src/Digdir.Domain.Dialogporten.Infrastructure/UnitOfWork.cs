using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Common.ReturnTypes;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.Abstractions.Features.Versionable;
using Digdir.Library.Entity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OneOf.Types;
using Polly;
using Polly.Contrib.WaitAndRetry;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork, IAsyncDisposable, IDisposable
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

    private IDbContextTransaction? _transaction;

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

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= await _dialogDbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<SaveChangesResult> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await SaveChangesAsync_Internal(cancellationToken);

            // If it is not a success, rollback the transaction
            await (result.IsT0
                ? CommitTransactionAsync(cancellationToken)
                : RollbackTransactionAsync(cancellationToken));

            return result;
        }
        catch (Exception)
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    private async Task<SaveChangesResult> SaveChangesAsync_Internal(CancellationToken cancellationToken)
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

    private async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.CommitAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    private async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
        {
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
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

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _transaction = null;
    }
}
