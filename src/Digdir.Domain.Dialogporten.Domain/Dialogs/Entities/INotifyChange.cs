namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public interface INotifyChange
{
    void Created(object? child, DateTimeOffset utcNow);
    void Updated(object? child, DateTimeOffset utcNow);
    void Deleted(object? child, DateTimeOffset utcNow);
}