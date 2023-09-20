using Digdir.Library.Entity.Abstractions.Features.Aggregate;
using Digdir.Library.Entity.Abstractions.Features.Immutable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class DialogSearchTag : IImmutableEntity
{
    private string _value = null!;
    public Guid Id { get; set; }

    public string Value
    {
        get => _value;
        set => _value = value.Trim().ToLower();
    }

    public DateTimeOffset CreatedAt { get; set; }
    
    [AggregateParent]
    public DialogEntity Dialog { get; set; } = null!;
    public Guid DialogId { get; set; }
}