using Digdir.Library.Entity.Abstractions.Features.Lookup;

namespace Digdir.Domain.Dialogporten.Domain.Attachments;

public class AttachmentUrlConsumerType : AbstractLookupEntity<AttachmentUrlConsumerType, AttachmentUrlConsumerType.Values>
{
    public AttachmentUrlConsumerType(Values id) : base(id) { }
    public override AttachmentUrlConsumerType MapValue(Values id) => new(id);

    public enum Values
    {
        Gui = 1,
        Api = 2
    }
}
