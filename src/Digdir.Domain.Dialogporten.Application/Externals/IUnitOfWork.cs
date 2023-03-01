namespace Digdir.Domain.Dialogporten.Application.Externals;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}