using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Attachments;

public class DialogAttachmentUrlConsumerType : AbstractLookupEntity<DialogAttachmentUrlConsumerType, DialogAttachmentUrlConsumerType.Values>
{
    public DialogAttachmentUrlConsumerType(Values id) : base(id) { }
    public override DialogAttachmentUrlConsumerType MapValue(Values id) => new(id);

    public enum Values
    {
        Gui = 1,
        Api = 2
    }
}
