using Digdir.Library.Entity.Abstractions;
using Digdir.Library.Entity.Abstractions.Features.SoftDeletable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities.Labels;

// Amund: en const for predefinerte labels virker som rett tanke, men hvor skal den ligge?
// Vil egt bruke Enum, men siden det er planlagt for egendefinerte labels så burde det bære være strings?
// Føles feil, liker det egt ikkje. vil helst ha type-safety men ser ikke helt hvordan
// Wrapper/Union? samling av const + enum, men en Value methode? Virker enda mer feil 
public static class DialogLabelName
{
    public const string Trashcan = "dp:trashcan";
    public const string Archive = "dp:archive";
}

// Amund: Matcher ikkje 100% med diagram/forklaring fra Bjørn.
// men trur Magnus sa at det er slik jeg skal bruke interfacene.
public sealed class DialogLabel : IEntity, ISoftDeletableEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public bool Deleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    public string LabelName { get; set; } = null!;
}
