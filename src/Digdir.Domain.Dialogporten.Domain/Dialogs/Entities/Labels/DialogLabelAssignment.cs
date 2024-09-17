using System.Reflection.Emit;
using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.Identifiable;
using Digdir.Library.Entity.Abstractions.Features.SoftDeletable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Labels;

// Amund: er dette rett? he ikkje fonne en koblings entitet å sammenligne mæ endå. 
public class DialogLabelAssignment : IIdentifiableEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }

    // public Guid LabelId { get; set; }
    public Label Label { get; set; }

    // public Guid DialogId { get; set; }
    public DialogEntity Dialog { get; set; } = null!;

    public string AssignedByParty { get; set; } = null!;
    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}
