namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IUnitOfWork
{
    IUnitOfWork WithoutAuditableSideEffects();
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}