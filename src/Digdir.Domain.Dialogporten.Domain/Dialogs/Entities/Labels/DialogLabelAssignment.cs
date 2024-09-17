using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using Digdir.Library.Entity.Abstractions.Features.SoftDeletable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Labels;

// Amund: Er IEntity rett her? he ikkje fonne en koblings entitet å sammenligne mæ endå. 
public class DialogLabelAssignment : IIdentifiableEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }

    public Guid LabelId { get; set; }

    public Guid DialogId { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
