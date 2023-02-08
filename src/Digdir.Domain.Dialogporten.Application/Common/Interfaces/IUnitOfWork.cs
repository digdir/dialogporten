namespace Digdir.Domain.Dialogporten.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken= default);
}