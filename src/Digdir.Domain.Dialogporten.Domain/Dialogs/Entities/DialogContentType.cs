using Digdir.Domain.Dialogporten.Domain.Localizations;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

public class DialogContentType : AbstractLookupEntity<DialogContentType, DialogContentType.Values>
{
    public DialogContentType(Values id) : base(id) { }
    public enum Values
    {
        Title = 1,
        SenderName = 2,
        Summary = 3,
        AdditionalInfo = 4,
    }

    public bool Required { get; private init; }
    public bool RenderAsHtml { get; private init; }
    public bool OutputInList { get; private init; }
    public int MaxLength { get; private init; }

    public override DialogContentType MapValue(Values id) => id switch
    {
        Values.Title => new(id)
        {
            Required = true,
            RenderAsHtml = false,
            MaxLength = 200,
            OutputInList = true
        },
        Values.SenderName => new(id)
        {
            Required = false,
            RenderAsHtml = false,
            MaxLength = 200,
            OutputInList = true
        },
        Values.Summary => new(id)
        {
            Required = true,
            RenderAsHtml = false,
            MaxLength = 200,
            OutputInList = true
        },
        Values.AdditionalInfo => new(id)
        {
            Required = false,
            RenderAsHtml = true,
            MaxLength = 1023,
            OutputInList = false
        },
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
    };
}

public class DialogContent : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    // === Dependent relationships ===
    public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public DialogContentType.Values TypeId { get; set; }
    public DialogContentType Type { get; set; } = null!;

    // === Principal relationships ===
    public List<DialogContentValue> Values { get; set; } = new();
}

public class DialogContentValue : LocalizationSet
{
    public Guid DialogContentId { get; set; }
    public DialogContent DialogContent { get; set; } = null!;
}
