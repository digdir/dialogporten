using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.DomainEvents;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly DialogueDbContext _dialogueDbContext;
    private readonly DomainEventPublisher _domainEventPublisher;

    public UnitOfWork(DialogueDbContext dialogueDbContext, DomainEventPublisher domainEventPublisher)
    {
        _dialogueDbContext = dialogueDbContext ?? throw new ArgumentNullException(nameof(dialogueDbContext));
        _domainEventPublisher = domainEventPublisher ?? throw new ArgumentNullException(nameof(domainEventPublisher));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Eject if domain errors
        // TODO: Handle domain events
        // TODO: Get the correct user id.
        var now = DateTimeOffset.UtcNow;
        _dialogueDbContext.ChangeTracker.HandleAuditableEntities(now);
        foreach (var domainEvent in _domainEventPublisher.GetDomainEvents())
        {
            domainEvent.OccuredAtUtc = now;
        }
        return _dialogueDbContext.SaveChangesAsync(cancellationToken);
    }
}
