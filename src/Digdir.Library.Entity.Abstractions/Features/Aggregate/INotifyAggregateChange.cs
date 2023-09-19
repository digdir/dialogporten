namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public interface INotifyAggregateChange :
    INotifyAggregateCreated,
    INotifyAggregateUpdated,
    INotifyAggregateDeleted
{ }

public interface INotifyAggregateCreated
{
    void Created(AggregateNode self, DateTimeOffset utcNow);
}

public interface INotifyAggregateUpdated
{
    void Updated(AggregateNode self, DateTimeOffset utcNow);
}

public interface INotifyAggregateDeleted
{
    void Deleted(AggregateNode self, DateTimeOffset utcNow);
}
