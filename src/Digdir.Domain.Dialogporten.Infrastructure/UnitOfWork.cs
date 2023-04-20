using Digdir.Domain.Dialogporten.Application.Externals;
using Digdir.Domain.Dialogporten.Infrastructure.Persistence;
using Digdir.Library.Entity.EntityFrameworkCore;

namespace Digdir.Domain.Dialogporten.Infrastructure;

internal sealed class UnitOfWork : IUnitOfWork
{
    private static readonly Guid MockUserId = Guid.Parse("9b0f57e3-4bc8-449f-8bc1-f29b3f992333");
    private readonly DialogueDbContext _dialogueDbContext;

    public UnitOfWork(DialogueDbContext dialogueDbContext)
    {
        _dialogueDbContext = dialogueDbContext ?? throw new ArgumentNullException(nameof(dialogueDbContext));
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Eject if domain errors
        // TODO: Handle domain events
        // TODO: Get the correct user id.
        _dialogueDbContext.ChangeTracker.HandleAuditableEntities(MockUserId, DateTimeOffset.UtcNow);
        return _dialogueDbContext.SaveChangesAsync(cancellationToken);
    }
}