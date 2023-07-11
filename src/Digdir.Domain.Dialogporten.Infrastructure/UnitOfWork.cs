using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly DialogDbContext _dialogDbContext;
    private readonly ITransactionTime _transactionTime;
    private readonly IDomainContext _domainContext;

    public UnitOfWork(DialogDbContext dialogDbContext, ITransactionTime transactionTime, IDomainContext domainContext)
    {
        _dialogDbContext = dialogDbContext ?? throw new ArgumentNullException(nameof(dialogDbContext));
        _transactionTime = transactionTime ?? throw new ArgumentNullException(nameof(transactionTime));
        _domainContext = domainContext ?? throw new ArgumentNullException(nameof(domainContext));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        _domainContext.EnsureValidState();
        // TODO: '_domainContext.EnsureValidState()' OR should we allow the application to proceed with invalid state?
        //if (!_domainContext.IsValid)
        //{
        //    return Task.CompletedTask;
        //}

        if (!_dialogDbContext.ChangeTracker.HasChanges())
        {
            return Task.CompletedTask;
        }

        _dialogDbContext.ChangeTracker.HandleAuditableEntities(_transactionTime.Value);
        return _dialogDbContext.SaveChangesAsync(cancellationToken);
    }
}
