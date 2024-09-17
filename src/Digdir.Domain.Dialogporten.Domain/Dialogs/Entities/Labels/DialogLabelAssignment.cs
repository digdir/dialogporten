using Digdir.Library.Entity.Abstractions;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Labels;

// Amund: Er IEntity rett her? he ikkje fonne en koblings entitet å sammenligne mæ endå. 
public class DialogLabelAssignment : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
