using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly DialogDbContext _dialogDbContext;
    private readonly DomainEventPublisher _domainEventPublisher;

    public UnitOfWork(DialogDbContext dialogDbContext, DomainEventPublisher domainEventPublisher)
    {
        _dialogDbContext = dialogDbContext ?? throw new ArgumentNullException(nameof(dialogDbContext));
        _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Eject if domain errors
        // TODO: Handle domain events
        // TODO: Get the correct user id.
        var now = DateTimeOffset.UtcNow;
        _dialogDbContext.ChangeTracker.HandleAuditableEntities(now);
        foreach (var domainEvent in _domainEventPublisher.GetDomainEvents())
        {
            domainEvent.OccuredAtUtc = now;
        }
        return _dialogDbContext.SaveChangesAsync(cancellationToken);
    }
}
