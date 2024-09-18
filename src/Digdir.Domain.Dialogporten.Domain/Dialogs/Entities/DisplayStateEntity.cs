using Digdir.Library.Entity.Abstractions.Features.Identifiable;

namespace Digdir.Domain.Dialogporten.Domain.Dialogs.Entities;

// Amund: Føles ikke riktig
public enum DisplayStateValue
{
    /// <summary>
    /// Default state
    /// </summary>
    Default = 1,
    /// <summary>
    /// Used to indicate the dialog is in the "trash"
    /// </summary>
    Deleted = 2,
    /// <summary>
    /// Used to indicate the dialog is in "archive"
    /// </summary>
    Archived = 3
}

public class DisplayStateEntity : IIdentifiableEntity
{
    public Guid Id { get; set; }

    public Guid DialogId { get; set; }

    // Amund: DialogStatus brukes ved Id + Values. skal det samme gjøres her?
    public DisplayStateValue State { get; set; }

    // public GlobalDisplayState.Values DisplayStateId { get; set; }
    // public GlobalDisplayState DisplayState { get; set; } = null!;
}
