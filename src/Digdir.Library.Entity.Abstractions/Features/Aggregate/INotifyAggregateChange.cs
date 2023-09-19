namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public interface INotifyAggregateChange :
    INotifyAggregateCreated,
    INotifyAggregateUpdated,
    INotifyAggregateDeleted
{ }

public interface INotifyAggregateCreated
{
    void OnCreate(AggregateNode self, DateTimeOffset utcNow);
}

public interface INotifyAggregateUpdated
{
    void OnUpdate(AggregateNode self, DateTimeOffset utcNow);
}

public interface INotifyAggregateDeleted
{
    void OnDelete(AggregateNode self, DateTimeOffset utcNow);
}
