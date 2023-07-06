using Digdir.Domain.Dialogporten.Application.Common;
using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly DialogDbContext _dialogDbContext;
    private readonly ITransactionTime _transactionTime;

    public UnitOfWork(DialogDbContext dialogDbContext, ITransactionTime transactionTime)
    {
        _dialogDbContext = dialogDbContext ?? throw new ArgumentNullException(nameof(dialogDbContext));
        _transactionTime = transactionTime ?? throw new ArgumentNullException(nameof(transactionTime));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!_dialogDbContext.ChangeTracker.HasChanges())
        {
            return Task.CompletedTask;
        }

        // TODO: Eject if domain errors
        _dialogDbContext.ChangeTracker.HandleAuditableEntities(_transactionTime.Value);
        return _dialogDbContext.SaveChangesAsync(cancellationToken);
    }
}
